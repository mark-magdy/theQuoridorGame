using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.DAL.Repositories.Interfaces;
using QuoridorBackend.Domain.DTOs.Game;
using QuoridorBackend.Domain.Entities;
using QuoridorBackend.Domain.Enums;
using QuoridorBackend.Domain.Models;
using System.Text.Json;

namespace QuoridorBackend.BLL.Services;

public class GameService : IGameService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGameRepository _gameRepository;
    private readonly IGameValidationService _validationService;

    public GameService(IUnitOfWork unitOfWork, IGameRepository gameRepository, IGameValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _gameRepository = gameRepository;
        _validationService = validationService;
    }

    public async Task<CreateGameResponse> CreateBotGameAsync(Guid userId, CreateBotGameRequest request)
    {
        // Initialize game state
        var gameState = InitializeGameState(request.Settings, request.BotDifficulty, request.BotPlayerIndex, userId);
        
        // Create game entity
        var game = new Game
        {
            Id = Guid.NewGuid(),
            GameStateJson = JsonSerializer.Serialize(gameState),
            SettingsJson = JsonSerializer.Serialize(request.Settings),
            Status = "playing",
            CreatedBy = userId,
            IsPrivate = true,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();

        return new CreateGameResponse
        {
            GameId = game.Id,
            GameState = gameState
        };
    }

    public async Task<MakeMoveResponse> MakeMoveAsync(Guid gameId, Guid userId, MakeMoveRequest request)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            return new MakeMoveResponse { IsValid = false, Error = "Game not found" };

        if (game.CreatedBy != userId)
            return new MakeMoveResponse { IsValid = false, Error = "Unauthorized" };

        var gameState = JsonSerializer.Deserialize<GameState>(game.GameStateJson);
        if (gameState == null)
            return new MakeMoveResponse { IsValid = false, Error = "Invalid game state" };

        // Validate and apply the player's move
        var validationResult = ValidateAndApplyMove(gameState, request.Move);
        if (!validationResult.IsValid)
            return validationResult;

        Move? botMove = null;
        
        // Check if it's a bot's turn and make bot move
        if (gameState.GameStatus == GameStatus.Playing)
        {
            var currentPlayer = gameState.Players[gameState.CurrentPlayerIndex];
            if (currentPlayer.Type == PlayerType.Bot)
            {
                botMove = GenerateBotMove(gameState, currentPlayer);
                if (botMove != null)
                {
                    var botMoveResult = ValidateAndApplyMove(gameState, botMove);
                    if (!botMoveResult.IsValid)
                    {
                        botMove = null; // Bot move was invalid, skip it
                    }
                }
            }
        }

        // Update game in database
        game.GameStateJson = JsonSerializer.Serialize(gameState);
        game.UpdatedAt = DateTime.UtcNow;

        bool gameEnded = false;
        int? winnerId = null;

        if (gameState.GameStatus == GameStatus.Finished)
        {
            game.Status = "finished";
            game.FinishedAt = DateTime.UtcNow;
            gameEnded = true;
            winnerId = gameState.Winner;
        }

        await _unitOfWork.SaveChangesAsync();

        return new MakeMoveResponse
        {
            IsValid = true,
            GameState = gameState,
            BotMove = botMove,
            GameEnded = gameEnded,
            WinnerId = winnerId
        };
    }

    public async Task<bool> DeleteGameAsync(Guid gameId, Guid userId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null || game.CreatedBy != userId)
            return false;

        await _gameRepository.DeleteAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<GameDto?> GetGameAsync(Guid gameId, Guid userId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            return null;

        // Verify user has access to this game
        if (game.CreatedBy != userId)
            return null;

        var gameState = JsonSerializer.Deserialize<GameState>(game.GameStateJson);
        var settings = JsonSerializer.Deserialize<GameSettings>(game.SettingsJson);

        return new GameDto
        {
            Id = game.Id,
            GameState = gameState!,
            Settings = settings!,
            Status = game.Status,
            CreatedAt = game.CreatedAt,
            StartedAt = game.StartedAt,
            FinishedAt = game.FinishedAt,
            IsPrivate = game.IsPrivate
        };
    }

    public async Task<IEnumerable<GameDto>> GetUserGamesAsync(Guid userId)
    {
        var games = await _gameRepository.GetAllAsync();
        var userGames = games.Where(g => g.CreatedBy == userId && g.Status != "finished")
                             .OrderByDescending(g => g.UpdatedAt);

        var gameDtos = new List<GameDto>();
        foreach (var game in userGames)
        {
            var gameState = JsonSerializer.Deserialize<GameState>(game.GameStateJson);
            var settings = JsonSerializer.Deserialize<GameSettings>(game.SettingsJson);

            gameDtos.Add(new GameDto
            {
                Id = game.Id,
                GameState = gameState!,
                Settings = settings!,
                Status = game.Status,
                CreatedAt = game.CreatedAt,
                StartedAt = game.StartedAt,
                FinishedAt = game.FinishedAt,
                IsPrivate = game.IsPrivate
            });
        }

        return gameDtos;
    }

    public async Task<IEnumerable<GameDto>> GetUserFinishedGamesAsync(Guid userId)
    {
        var games = await _gameRepository.GetAllAsync();
        var userGames = games.Where(g => g.CreatedBy == userId && g.Status == "finished")
                             .OrderByDescending(g => g.FinishedAt);

        var gameDtos = new List<GameDto>();
        foreach (var game in userGames)
        {
            var gameState = JsonSerializer.Deserialize<GameState>(game.GameStateJson);
            var settings = JsonSerializer.Deserialize<GameSettings>(game.SettingsJson);

            gameDtos.Add(new GameDto
            {
                Id = game.Id,
                GameState = gameState!,
                Settings = settings!,
                Status = game.Status,
                CreatedAt = game.CreatedAt,
                StartedAt = game.StartedAt,
                FinishedAt = game.FinishedAt,
                IsPrivate = game.IsPrivate
            });
        }

        return gameDtos;
    }

    public async Task<AvailableMovesResponse> GetAvailableMovesAsync(Guid gameId, Guid userId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null || game.CreatedBy != userId)
            return new AvailableMovesResponse();

        var gameState = JsonSerializer.Deserialize<GameState>(game.GameStateJson);
        if (gameState == null || gameState.GameStatus != GameStatus.Playing)
            return new AvailableMovesResponse();

        var currentPlayer = gameState.Players[gameState.CurrentPlayerIndex];
        
        // Get valid moves
        var validPositions = _validationService.GetValidPawnMoves(gameState, currentPlayer.Id);
        var validWalls = _validationService.GetValidWallPlacements(gameState, currentPlayer.Id);

        return new AvailableMovesResponse
        {
            ValidPawnMoves = validPositions.Select(p => p.ToAlgebraicNotation()).ToList(),
            ValidWallPlacements = validWalls.Select(w => w.GetWallId()).ToList()
        };
    }

    #region Private Helper Methods

    private GameState InitializeGameState(GameSettings settings, BotDifficulty botDifficulty, int botPlayerIndex, Guid userId)
    {
        var gameState = new GameState
        {
            BoardSize = settings.BoardSize,
            CurrentPlayerIndex = 0,
            GameStatus = GameStatus.Playing,
            Players = new List<Player>(),
            Walls = new List<Wall>(),
            MoveHistory = new List<Move>(),
            HistoryIndex = 0
        };

        // Determine walls per player based on player count
        int wallsPerPlayer = settings.PlayerCount switch
        {
            2 => 10,
            3 => 7,
            4 => 5,
            _ => 10
        };

        // Create players
        for (int i = 0; i < settings.PlayerCount; i++)
        {
            var isBot = i == botPlayerIndex;
            var player = new Player
            {
                Id = i,
                Color = (PlayerColor)i,
                WallsRemaining = wallsPerPlayer,
                Type = isBot ? PlayerType.Bot : PlayerType.Human,
                BotDifficulty = isBot ? botDifficulty : null,
                Name = isBot ? $"Bot ({botDifficulty})" : "You",
                UserId = isBot ? null : userId.ToString()
            };

            // Set starting position and goal based on player index
            SetPlayerStartPositionAndGoal(player, i, settings.BoardSize, settings.PlayerCount);
            gameState.Players.Add(player);
        }

        return gameState;
    }

    private void SetPlayerStartPositionAndGoal(Player player, int playerIndex, int boardSize, int playerCount)
    {
        int mid = boardSize / 2;

        switch (playerCount)
        {
            case 2:
                if (playerIndex == 0)
                {
                    player.Position = new Position { Row = boardSize - 1, Col = mid };
                    player.GoalRow = 0;
                }
                else
                {
                    player.Position = new Position { Row = 0, Col = mid };
                    player.GoalRow = boardSize - 1;
                }
                break;

            case 4:
                switch (playerIndex)
                {
                    case 0: // Bottom
                        player.Position = new Position { Row = boardSize - 1, Col = mid };
                        player.GoalRow = 0;
                        break;
                    case 1: // Top
                        player.Position = new Position { Row = 0, Col = mid };
                        player.GoalRow = boardSize - 1;
                        break;
                    case 2: // Left
                        player.Position = new Position { Row = mid, Col = 0 };
                        player.GoalRow = -1; // Special case for horizontal goal
                        break;
                    case 3: // Right
                        player.Position = new Position { Row = mid, Col = boardSize - 1 };
                        player.GoalRow = -1; // Special case for horizontal goal
                        break;
                }
                break;

            default: // 3 players or other configurations
                if (playerIndex == 0)
                {
                    player.Position = new Position { Row = boardSize - 1, Col = mid };
                    player.GoalRow = 0;
                }
                else if (playerIndex == 1)
                {
                    player.Position = new Position { Row = 0, Col = 0 };
                    player.GoalRow = boardSize - 1;
                }
                else
                {
                    player.Position = new Position { Row = 0, Col = boardSize - 1 };
                    player.GoalRow = boardSize - 1;
                }
                break;
        }
    }

    private MakeMoveResponse ValidateAndApplyMove(GameState gameState, Move move)
    {
        if (gameState.GameStatus != GameStatus.Playing)
            return new MakeMoveResponse { IsValid = false, Error = "Game is not in playing state" };

        var currentPlayer = gameState.Players[gameState.CurrentPlayerIndex];
        if (move.PlayerId != currentPlayer.Id)
            return new MakeMoveResponse { IsValid = false, Error = "Not your turn" };

        // Validate move using validation service
        if (move.Type == MoveType.Pawn)
        {
            if (move.To == null)
                return new MakeMoveResponse { IsValid = false, Error = "Invalid pawn move - no destination" };

            if (!_validationService.IsValidPawnMove(gameState, move.PlayerId, move.To))
                return new MakeMoveResponse { IsValid = false, Error = "Invalid pawn move - not allowed" };

            // Update player position
            currentPlayer.Position = move.To;
            
            // Check if player reached goal
            if (_validationService.IsGameWon(gameState, currentPlayer.Id))
            {
                gameState.GameStatus = GameStatus.Finished;
                gameState.Winner = currentPlayer.Id;
            }
        }
        else if (move.Type == MoveType.Wall)
        {
            if (move.Wall == null)
                return new MakeMoveResponse { IsValid = false, Error = "Invalid wall placement - no wall data" };
                
            if (currentPlayer.WallsRemaining <= 0)
                return new MakeMoveResponse { IsValid = false, Error = "No walls remaining" };

            if (!_validationService.IsValidWallPlacement(gameState, move.Wall))
                return new MakeMoveResponse { IsValid = false, Error = "Invalid wall placement - blocks path or overlaps" };

            gameState.Walls.Add(move.Wall);
            currentPlayer.WallsRemaining--;
        }

        // Add move to history
        gameState.MoveHistory.Add(move);
        gameState.HistoryIndex++;

        // Move to next player
        if (gameState.GameStatus == GameStatus.Playing)
        {
            gameState.CurrentPlayerIndex = (gameState.CurrentPlayerIndex + 1) % gameState.Players.Count;
        }

        return new MakeMoveResponse
        {
            IsValid = true,
            GameState = gameState
        };
    }

    // private bool HasReachedGoal(Player player, int boardSize)
    // {
    //     if (player.GoalRow == -1)
    //     {
    //         // Horizontal goal (for 4-player game)
    //         return player.Position.Col == 0 || player.Position.Col == boardSize - 1;
    //     }
    //     return player.Position.Row == player.GoalRow;
    // }

    private Move? GenerateBotMove(GameState gameState, Player botPlayer)
    {
        // Simple bot implementation - just move towards goal
        // TODO: Implement proper bot AI based on difficulty level
        
        var currentPos = botPlayer.Position;
        var possibleMoves = new List<Position>();

        // Try to move towards goal
        if (botPlayer.GoalRow != -1)
        {
            // Vertical goal
            if (currentPos.Row > botPlayer.GoalRow)
                possibleMoves.Add(new Position { Row = currentPos.Row - 1, Col = currentPos.Col });
            else if (currentPos.Row < botPlayer.GoalRow)
                possibleMoves.Add(new Position { Row = currentPos.Row + 1, Col = currentPos.Col });
        }

        // Add lateral moves
        possibleMoves.Add(new Position { Row = currentPos.Row, Col = currentPos.Col + 1 });
        possibleMoves.Add(new Position { Row = currentPos.Row, Col = currentPos.Col - 1 });

        // Filter valid moves (within board bounds)
        var validMoves = possibleMoves.Where(p => 
            p.Row >= 0 && p.Row < gameState.BoardSize && 
            p.Col >= 0 && p.Col < gameState.BoardSize).ToList();

        if (validMoves.Any())
        {
            // For now, just pick the first valid move
            // TODO: Implement smarter bot logic based on difficulty
            var selectedMove = validMoves.First();
            
            return new Move
            {
                Type = MoveType.Pawn,
                PlayerId = botPlayer.Id,
                From = currentPos,
                To = selectedMove,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        return null;
    }

    #endregion
}
