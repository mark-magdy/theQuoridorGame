using QuoridorBackend.Domain.DTOs;
using QuoridorBackend.Domain.DTOs.Game;
using QuoridorBackend.Domain.Models;
using QuoridorBackend.Domain.Enums;
using QuoridorBackend.Domain.Entities;
using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.DAL.Repositories.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace QuoridorBackend.BLL.Services
{
    public class GameRoomService : IGameRoomService
    {
        private readonly IGameService _gameService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDatabase _redis;

        private const string RoomsKeyPrefix = "room:";
        private const string GameStatesKeyPrefix = "gamestate:";
        private const string RoomsSetKey = "rooms:all";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GameRoomService(IGameService gameService, IUnitOfWork unitOfWork, IConnectionMultiplexer redis)
        {
            _gameService = gameService;
            _unitOfWork = unitOfWork;
            _redis = redis.GetDatabase(0); // Explicitly use database 0

            // Diagnostic: Check Redis connection and keys
            var endpoint = redis.GetEndPoints().FirstOrDefault();
            var keyCount = _redis.Execute("DBSIZE");
            Console.WriteLine($"[GameRoomService] Instance created - Redis: {endpoint}, IsConnected: {redis.IsConnected}, DB: 0, Keys in DB: {keyCount}");
        }

        public async Task<RoomDto> CreateRoom(string hostUserId, string connectionId, int maxPlayers)
        {
            Console.WriteLine($"[GameRoomService] CreateRoom called - User: {hostUserId}, ConnectionId: {connectionId}, MaxPlayers: {maxPlayers}");

            if (maxPlayers < 2 || maxPlayers > 4)
                throw new ArgumentException("Max players must be between 2 and 4");

            var roomId = await GenerateRoomId();
            Console.WriteLine($"[GameRoomService] Generated roomId: '{roomId}'");
            var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(hostUserId));
            if (user == null)
                throw new InvalidOperationException("User not found");

            var room = new GameRoom
            {
                RoomId = roomId,
                HostUserId = hostUserId,
                MaxPlayers = maxPlayers,
                CurrentPlayers = 1,
                Status = "Waiting",
                CreatedAt = DateTime.UtcNow,
                Players = new List<GameRoomPlayer>
                {
                    new GameRoomPlayer
                    {
                        RoomId = roomId,
                        UserId = hostUserId,
                        ConnectionId = connectionId,
                        JoinedAt = DateTime.UtcNow,
                        IsReady = true
                    }
                }
            };

            await SaveRoomToRedis(room);
            await _redis.SetAddAsync(RoomsSetKey, roomId);
            Console.WriteLine($"[GameRoomService] Room created in Redis: {roomId}");

            // Verify room was saved
            var temp = await GetRoomFromRedis(roomId);
            if (temp == null)
            {
                Console.WriteLine($"[GameRoomService] WARNING: Room '{roomId}' not found in Redis immediately after creation!");
            }
            else
            {
                Console.WriteLine($"[GameRoomService] ✓ Verification successful - Retrieved room from Redis: {temp.RoomId}, Players: {temp.Players.Count}");
            }
            return MapToRoomDto(room, await GetUsernames(room.Players.Select(p => p.UserId).ToList()));
        }

        public async Task<RoomDto> JoinRoom(string roomId, string userId, string connectionId)
        {
            Console.WriteLine($"[GameRoomService] Attempting to join room: '{roomId}' (Length: {roomId?.Length}, Trimmed: '{roomId?.Trim()}'), User: {userId}");

            var trimmedRoomId = roomId?.Trim() ?? string.Empty;
            var room = await GetRoomFromRedis(trimmedRoomId);

            if (room == null)
            {
                Console.WriteLine($"[GameRoomService] Room '{trimmedRoomId}' not found in Redis.");
                throw new InvalidOperationException("Room not found");
            }

            if (room.Status != "Waiting")
                throw new InvalidOperationException("Room is not accepting new players");

            // Check if player is already in room (reconnection case)
            var existingPlayer = room.Players.FirstOrDefault(p => p.UserId == userId);
            if (existingPlayer != null)
            {
                // Player reconnecting - update their connection ID
                Console.WriteLine($"[GameRoomService] Player <> {userId} reconnecting to room - updating connectionId");
                existingPlayer.ConnectionId = connectionId;

                // Save the updated room to Redis
                await SaveRoomToRedis(room);

                // Verify the save
                var verifyRoom = await GetRoomFromRedis(trimmedRoomId);
                Console.WriteLine($"[GameRoomService] Player reconnected successfully to room '{trimmedRoomId}', Verification: {verifyRoom != null}");

                // Return the updated room DTO
                return MapToRoomDto(room, await GetUsernames(room.Players.Select(p => p.UserId).ToList()));
            }

            // Check if room is full BEFORE adding new player
            if (room.CurrentPlayers >= room.MaxPlayers)
                throw new InvalidOperationException("Room is full");

            // Add new player
            var player = new GameRoomPlayer
            {
                RoomId = trimmedRoomId,
                UserId = userId,
                ConnectionId = connectionId,
                JoinedAt = DateTime.UtcNow,
                IsReady = true
            };

            room.Players.Add(player);
            room.CurrentPlayers++;

            // Save to Redis
            await SaveRoomToRedis(room);

            // Verify the save worked
            var savedRoom = await GetRoomFromRedis(trimmedRoomId);
            if (savedRoom == null || savedRoom.CurrentPlayers != room.CurrentPlayers)
            {
                Console.WriteLine($"[GameRoomService] ERROR: Room save verification failed! Expected {room.CurrentPlayers} players, got {savedRoom?.CurrentPlayers ?? 0}");
                throw new InvalidOperationException("Failed to save room state to cache");
            }

            Console.WriteLine($"[GameRoomService] Successfully joined room '{trimmedRoomId}', Total players: {room.CurrentPlayers}");
            return MapToRoomDto(room, await GetUsernames(room.Players.Select(p => p.UserId).ToList()));
        }
        public async Task<RoomDto?> LeaveRoom(string roomId, string userId)
        {
            var room = await GetRoomFromRedis(roomId);
            if (room == null)
                return null;

            var player = room.Players.FirstOrDefault(p => p.UserId == userId);
            if (player == null)
                return null;

            room.Players.Remove(player);
            room.CurrentPlayers--;

            // If host leaves or room is empty, close the room
            if (userId == room.HostUserId || room.CurrentPlayers == 0)
            {
                await _redis.KeyDeleteAsync(RoomsKeyPrefix + roomId);
                await _redis.KeyDeleteAsync(GameStatesKeyPrefix + roomId);
                await _redis.SetRemoveAsync(RoomsSetKey, roomId);
                return null;
            }

            // Assign new host if current host left
            if (room.Players.Count > 0)
            {
                room.HostUserId = room.Players[0].UserId;
            }

            await SaveRoomToRedis(room);
            return MapToRoomDto(room, await GetUsernames(room.Players.Select(p => p.UserId).ToList()));
        }

        public async Task<GameDto> StartGame(string roomId, string userId)
        {
            var room = await GetRoomFromRedis(roomId);
            if (room == null)
                throw new InvalidOperationException("Room not found");

            if (room.HostUserId != userId)
                throw new InvalidOperationException("Only the host can start the game");

            if (room.CurrentPlayers < 2)
                throw new InvalidOperationException("Need at least 2 players to start");

            if (room.Status != "Waiting")
                throw new InvalidOperationException("Game already started");

            // Create multiplayer game with all players in the room
            var playerUserIds = room.Players.Select(p => Guid.Parse(p.UserId)).ToList();
            
            var settings = new GameSettings
            {
                BoardSize = 9,
                PlayerCount = room.CurrentPlayers,
                Theme = "light",
                ShowValidPaths = true
            };

            var gameResponse = await _gameService.CreateMultiplayerGameAsync(playerUserIds, settings);

            // Update room status
            room.Status = "InProgress";
            room.GameId = gameResponse.GameId.ToString(); // Store Guid as string

            // Get the created game and save to Redis
            var hostGuid = Guid.Parse(userId);
            var game = await _gameService.GetGameAsync(gameResponse.GameId, hostGuid);
            
            if (game != null)
            {
                await SaveGameStateToRedis(roomId, game);
            }

            await SaveRoomToRedis(room);
            return game ?? throw new InvalidOperationException("Failed to create game");
        }

        public async Task<GameDto> MakeMove(GameMoveDto moveDto, string userId)
        {
            var room = await GetRoomFromRedis(moveDto.RoomId);
            if (room == null)
                throw new InvalidOperationException("Room not found");

            if (room.Status != "InProgress")
                throw new InvalidOperationException("Game is not in progress");

            if (string.IsNullOrEmpty(room.GameId))
                throw new InvalidOperationException("No active game in this room");

            // Get current game state
            var gameState = await GetGameStateFromRedis(moveDto.RoomId);
            var gameGuid = Guid.Parse(room.GameId); // Parse Guid from string

            if (gameState == null)
            {
                gameState = await _gameService.GetGameAsync(gameGuid, Guid.Parse(userId));
                if (gameState != null)
                {
                    await SaveGameStateToRedis(moveDto.RoomId, gameState);
                }
            }

            if (gameState == null)
                throw new InvalidOperationException("Game state not found");

            // Make the move using the existing game service
            int playerIndex = room.Players.FindIndex(p => p.UserId == userId);
            var moveRequest = new MakeMoveRequest();
            if (moveDto.MoveType == "move")
            {
                moveRequest.Move = new Move
                {
                    Type = MoveType.Pawn,
                    PlayerId=playerIndex,
                    To = new Position
                    {
                        Row = moveDto.ToRow,
                        Col = moveDto.ToCol
                    }
                };
            }
            else if (moveDto.MoveType == "wall")
            {
                moveRequest.Move = new Move
                {
                    Type = MoveType.Wall,
                    PlayerId = playerIndex,
                    Wall = new Wall
                    {
                        Position = new Position
                        {
                            Row = moveDto.ToRow,
                            Col = moveDto.ToCol
                        },
                        IsHorizontal = moveDto.WallOrientation == "horizontal"
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Invalid move type");
            }

            var moveResponse = await _gameService.MakeMoveAsync(gameGuid, Guid.Parse(userId), moveRequest);

            if (!moveResponse.IsValid)
                throw new InvalidOperationException(moveResponse.Error ?? "Invalid move");

            // Get updated game state
            var updatedGameState = await _gameService.GetGameAsync(gameGuid, Guid.Parse(userId));
            if (updatedGameState != null)
            {
                await SaveGameStateToRedis(moveDto.RoomId, updatedGameState);

                // Check if game is complete
                if (updatedGameState.Status == "finished")
                {
                    room.Status = "Completed";
                    await SaveRoomToRedis(room);
                }
            }

            return updatedGameState ?? throw new InvalidOperationException("Failed to get updated game state");
        }

        public async Task HandlePlayerDisconnect(string userId, string connectionId)
        {
            Console.WriteLine($"[GameRoomService] HandlePlayerDisconnect called - User: {userId}, ConnectionId: {connectionId}");

            // DON'T automatically remove players on disconnect - SignalR reconnects frequently!
            // Only mark them as disconnected or implement a grace period
            // Rooms should only be deleted when players explicitly leave or after a timeout

            var allRoomIds = await _redis.SetMembersAsync(RoomsSetKey);
            Console.WriteLine($"[GameRoomService] Found {allRoomIds.Length} total rooms during disconnect handling");

            // For now, just log but don't delete rooms on disconnect
            foreach (var roomIdValue in allRoomIds)
            {
                var room = await GetRoomFromRedis(roomIdValue.ToString());
                if (room != null && room.Players.Any(p => p.ConnectionId == connectionId))
                {
                    Console.WriteLine($"[GameRoomService] Player was in room {room.RoomId}, but NOT removing (allowing reconnection)");
                    // TODO: Implement grace period or "away" status instead of immediate removal
                    // await LeaveRoom(room.RoomId, userId);
                }
            }
        }

        public async Task<RoomDto?> GetRoom(string roomId)
        {
            var room = await GetRoomFromRedis(roomId);
            if (room == null)
                return null;

            return MapToRoomDto(room, await GetUsernames(room.Players.Select(p => p.UserId).ToList()));
        }

        private async Task<string> GenerateRoomId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var roomId = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Ensure uniqueness
            var exists = await _redis.KeyExistsAsync(RoomsKeyPrefix + roomId);
            return exists ? await GenerateRoomId() : roomId;
        }

        private async Task<Dictionary<string, string>> GetUsernames(List<string> userIds)
        {
            var usernames = new Dictionary<string, string>();
            foreach (var userId in userIds)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(userId));
                usernames[userId] = user?.Username ?? "Unknown";
            }
            return usernames;
        }

        private RoomDto MapToRoomDto(GameRoom room, Dictionary<string, string> usernames)
        {
            return new RoomDto
            {
                RoomId = room.RoomId,
                HostUserId = room.HostUserId,
                MaxPlayers = room.MaxPlayers,
                CurrentPlayers = room.CurrentPlayers,
                Status = room.Status,
                Players = room.Players.Select(p => new RoomPlayerDto
                {
                    UserId = p.UserId,
                    Username = usernames.GetValueOrDefault(p.UserId, "Unknown"),
                    IsReady = p.IsReady,
                    IsHost = p.UserId == room.HostUserId
                }).ToList()
            };
        }

        private async Task SaveRoomToRedis(GameRoom room)
        {
            var json = JsonSerializer.Serialize(room, JsonOptions);
            var key = RoomsKeyPrefix + room.RoomId;
            Console.WriteLine($"[GameRoomService] Saving to Redis - Key: '{key}', JSON length: {json.Length}");
            var result = await _redis.StringSetAsync(key, json, TimeSpan.FromHours(24));
            Console.WriteLine($"[GameRoomService] Redis SET result: {result}");
        }

        private async Task<GameRoom?> GetRoomFromRedis(string roomId)
        {
            var key = RoomsKeyPrefix + roomId;
            Console.WriteLine($"[GameRoomService] Getting from Redis - Key: '{key}'");
            var json = await _redis.StringGetAsync(key);

            if (!json.HasValue)
            {
                Console.WriteLine($"[GameRoomService] Redis returned no value for key: '{key}'");

                // Debug: List all keys in Redis
                var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
                var allKeys = server.Keys(database: 0, pattern: RoomsKeyPrefix + "*").ToList();
                Console.WriteLine($"[GameRoomService] DEBUG: All room keys in Redis: {string.Join(", ", allKeys.Select(k => k.ToString()))}");

                return null;
            }

            Console.WriteLine($"[GameRoomService] Retrieved JSON from Redis - Length: {json.ToString().Length}, First 200 chars: {json.ToString().Substring(0, Math.Min(200, json.ToString().Length))}");

            try
            {
                var result = JsonSerializer.Deserialize<GameRoom>(json.ToString(), JsonOptions);
                Console.WriteLine($"[GameRoomService] Successfully deserialized room: {result?.RoomId}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameRoomService] Error deserializing room from Redis: {ex.Message}");
                Console.WriteLine($"[GameRoomService] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private async Task SaveGameStateToRedis(string roomId, GameDto gameState)
        {
            var json = JsonSerializer.Serialize(gameState, JsonOptions);
            await _redis.StringSetAsync(GameStatesKeyPrefix + roomId, json, TimeSpan.FromHours(24));
        }

        private async Task<GameDto?> GetGameStateFromRedis(string roomId)
        {
            var json = await _redis.StringGetAsync(GameStatesKeyPrefix + roomId);
            if (!json.HasValue)
                return null;
            try
            {
                return JsonSerializer.Deserialize<GameDto>(json.ToString(), JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameRoomService] Error deserializing game state from Redis: {ex.Message}");
                return null;
            }
        }
    }
}
