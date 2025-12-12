using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.Domain.DTOs;
using System.Security.Claims;

namespace QuoridorBackend.Api.Hubs
{
    /// <summary>
    /// SignalR hub for managing game rooms, player connections, and real-time game actions.
    /// </summary>
    [Authorize]
    public class GameHub : Hub
    {
        private readonly IGameRoomService _gameRoomService;
        private readonly ILogger<GameHub> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameHub"/> class.
        /// </summary>
        /// <param name="gameRoomService">Service for managing game rooms.</param>
        /// <param name="logger">Logger instance.</param>
        public GameHub(IGameRoomService gameRoomService, ILogger<GameHub> logger)
        {
            _gameRoomService = gameRoomService;
            _logger = logger;
        }

        private string GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                   ?? throw new HubException("User not authenticated");
        }

        /// <summary>
        /// Called when a client connects to the hub.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} connected with connection {Context.ConnectionId}");
            
            // Note: Clients should call RejoinRoom after reconnecting to restore group membership
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// </summary>
        /// <param name="exception">The exception that occurred during disconnect, if any.</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} disconnected");
            
            // Handle player disconnect - remove from room if in one
            await _gameRoomService.HandlePlayerDisconnect(userId, Context.ConnectionId);
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Creates a new game room.
        /// </summary>
        /// <param name="request">Room creation request data.</param>
        /// <returns>The created room details.</returns>
        public async Task<RoomDto> CreateRoom(CreateRoomDto request)
        {
            try
            {
                var userId = GetUserId();
                var room = await _gameRoomService.CreateRoom(userId, Context.ConnectionId, request.MaxPlayers);
                
                await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);
                _logger.LogInformation($"User {userId} created room {room.RoomId}");
                
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                throw new HubException($"Failed to create room: {ex.Message}");
            }
        }

        /// <summary>
        /// Joins an existing game room.
        /// </summary>
        /// <param name="request">Join room request data.</param>
        /// <returns>The joined room details.</returns>
        public async Task<RoomDto> JoinRoom(JoinRoomDto request)
        {
            try
            {
                var userId = GetUserId();
                
                // Join the room in the service first
                var room = await _gameRoomService.JoinRoom(request.RoomId, userId, Context.ConnectionId);
                
                // Add to SignalR group AFTER successful join
                await Groups.AddToGroupAsync(Context.ConnectionId, request.RoomId);
                
                _logger.LogInformation($"User {userId} joined room {request.RoomId}, Total players: {room.CurrentPlayers}");
                
                // Notify OTHER players in the room (exclude the one who just joined)
                // They get the room via return value, others get it via notification
                await Clients.OthersInGroup(request.RoomId).SendAsync("RoomUpdated", room);
                
                // Return the room to the joining player
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room");
                throw new HubException($"Failed to join room: {ex.Message}");
            }
        }

        /// <summary>
        /// Leaves the specified game room.
        /// </summary>
        /// <param name="roomId">The ID of the room to leave.</param>
        public async Task LeaveRoom(string roomId)
        {
            try
            {
                var userId = GetUserId();
                var room = await _gameRoomService.LeaveRoom(roomId, userId);
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                _logger.LogInformation($"User {userId} left room {roomId}");
                
                if (room != null)
                {
                    // Notify remaining players about the updated room state
                    await Clients.Group(roomId).SendAsync("RoomUpdated", room);
                }
                else
                {
                    // Room was closed
                    await Clients.Group(roomId).SendAsync("RoomClosed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room");
                throw new HubException($"Failed to leave room: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the game in the specified room.
        /// </summary>
        /// <param name="roomId">The ID of the room to start the game in.</param>
        public async Task StartGame(string roomId)
        {
            try
            {
                var userId = GetUserId();
                var gameState = await _gameRoomService.StartGame(roomId, userId);
                
                _logger.LogInformation($"Game started in room {roomId}");
                
                // Notify all players that game has started
                await Clients.Group(roomId).SendAsync("GameStarted", gameState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game");
                throw new HubException($"Failed to start game: {ex.Message}");
            }
        }

        /// <summary>
        /// Makes a move in the game.
        /// </summary>
        /// <param name="moveDto">Move data transfer object.</param>
        public async Task MakeMove(GameMoveDto moveDto)
        {
            Console.WriteLine($"User {GetUserId()} is making a move in room {moveDto.RoomId}");
            try
            {
                var userId = GetUserId();
                var gameState = await _gameRoomService.MakeMove(moveDto, userId);
                
                _logger.LogInformation($"Move made in room {moveDto.RoomId} by user {userId}");
                
                // Broadcast the updated game state to all players in the room
                await Clients.Group(moveDto.RoomId).SendAsync("GameStateUpdated", gameState);
                
                // Check if game is over
                if (gameState.Status == "Completed" || gameState.Status == "finished")
                {
                    await Clients.Group(moveDto.RoomId).SendAsync("GameEnded", gameState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making move");
                throw new HubException($"Failed to make move: {ex.Message}");
            }
        }

        /// <summary>
        /// Rejoins a previously joined room after reconnecting.
        /// </summary>
        /// <param name="roomId">The ID of the room to rejoin.</param>
        /// <returns>The room details if rejoin is successful; otherwise, null.</returns>
        public async Task<RoomDto?> RejoinRoom(string roomId)
        {
            try
            {
                var userId = GetUserId();
                
                // Update the connection ID in the service
                var room = await _gameRoomService.JoinRoom(roomId, userId, Context.ConnectionId);
                
                if (room == null)
                {
                    _logger.LogWarning($"User {userId} tried to rejoin non-existent room {roomId}");
                    return null;
                }
                
                // Re-add to SignalR group after reconnection
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                _logger.LogInformation($"User {userId} rejoined room {roomId} with new connection {Context.ConnectionId}");
                
                // Notify other players that this user reconnected
                await Clients.OthersInGroup(roomId).SendAsync("PlayerReconnected", new 
                { 
                    UserId = userId,
                    Room = room 
                });
                
                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejoining room");
                throw new HubException($"Failed to rejoin room: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a chat message to all users in the specified room.
        /// </summary>
        /// <param name="roomId">The ID of the room.</param>
        /// <param name="message">The chat message.</param>
        public async Task SendChatMessage(string roomId, string message)
        {
            try
            {
                var userId = GetUserId();
                var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                
                await Clients.Group(roomId).SendAsync("ChatMessage", new
                {
                    UserId = userId,
                    Username = username,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message");
                throw new HubException($"Failed to send message: {ex.Message}");
            }
        }
    }
}