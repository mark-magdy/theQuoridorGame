using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.Domain.Enums;
using QuoridorBackend.Domain.Models;
using System.Collections.Concurrent;

namespace QuoridorBackend.BLL.Services;

/// <summary>
/// Quoridor AI Bot Engine using Minimax with Alpha-Beta pruning.
/// Implements strategic move generation and sophisticated evaluation.
/// </summary>
public class BotEngine : IBotEngine
{
    private readonly IGameValidationService _validationService;
    
    // Transposition table for caching evaluated game states
    private readonly ConcurrentDictionary<ulong, TranspositionEntry> _transpositionTable;
    
    // Path cache to avoid recomputing shortest paths
    private readonly Dictionary<string, int> _pathCache;
    
    // Evaluation function weights (tuned for strong play)
    private const double W1_PATH_DIFF = 10.0;        // Primary: opponent path - my path
    private const double W2_WALL_DIFF = 2.0;         // Wall advantage
    private const double W3_PATH_FLEXIBILITY = 1.5;  // Alternative paths
    private const double W4_POSITIONAL = 1.0;        // Position quality
    private const double W5_WALL_EFFICIENCY = 3.0;   // Wall effectiveness
    
    // Search depth by difficulty
    private const int EASY_DEPTH = 1;
    private const int MEDIUM_DEPTH = 3;
    private const int HARD_MAX_DEPTH = 5;
    
    // Decisive score thresholds for early cutoffs
    private const double WINNING_SCORE = 10000.0;
    private const double LOSING_SCORE = -10000.0;
    private const double DECISIVE_THRESHOLD = 50.0;
    
    public BotEngine(IGameValidationService validationService)
    {
        _validationService = validationService;
        _transpositionTable = new ConcurrentDictionary<ulong, TranspositionEntry>();
        _pathCache = new Dictionary<string, int>();
    }
    
    public Move? GetBestMove(GameState gameState, int botPlayerId, BotDifficulty difficulty)
    {
        // Clear caches for new move search
        _pathCache.Clear();
        
        // Determine search depth based on difficulty
        int maxDepth = difficulty switch
        {
            BotDifficulty.Easy => EASY_DEPTH,
            BotDifficulty.Medium => MEDIUM_DEPTH,
            BotDifficulty.Hard => HARD_MAX_DEPTH,
            _ => MEDIUM_DEPTH
        };
        
        // For Hard mode, use iterative deepening
        if (difficulty == BotDifficulty.Hard)
        {
            return GetBestMoveIterativeDeepening(gameState, botPlayerId, maxDepth, difficulty);
        }
        
        // For Easy/Medium, use fixed depth search
        var (bestMove, _) = MinimaxAlphaBeta(gameState, botPlayerId, maxDepth, 
            double.NegativeInfinity, double.PositiveInfinity, true, difficulty);
        
        return bestMove;
    }
    
    #region Minimax with Alpha-Beta Pruning
    
    /// <summary>
    /// Iterative deepening for Hard mode - search progressively deeper depths.
    /// </summary>
    private Move? GetBestMoveIterativeDeepening(GameState gameState, int botPlayerId, 
        int maxDepth, BotDifficulty difficulty)
    {
        Move? bestMove = null;
        
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            var (move, score) = MinimaxAlphaBeta(gameState, botPlayerId, depth,
                double.NegativeInfinity, double.PositiveInfinity, true, difficulty);
            
            if (move != null)
            {
                bestMove = move;
                
                // Early exit if we found a winning move
                if (score >= WINNING_SCORE - 100)
                    break;
            }
        }
        
        return bestMove;
    }
    
    /// <summary>
    /// Minimax algorithm with Alpha-Beta pruning.
    /// </summary>
    private (Move? bestMove, double score) MinimaxAlphaBeta(
        GameState gameState, 
        int botPlayerId, 
        int depth, 
        double alpha, 
        double beta, 
        bool isMaximizing,
        BotDifficulty difficulty)
    {
        // Terminal conditions
        if (depth == 0 || gameState.GameStatus != GameStatus.Playing)
        {
            return (null, EvaluatePosition(gameState, botPlayerId, difficulty));
        }
        
        // Check transposition table (Hard mode only)
        if (difficulty == BotDifficulty.Hard)
        {
            ulong hash = ComputeStateHash(gameState);
            if (_transpositionTable.TryGetValue(hash, out var entry) && entry.Depth >= depth)
            {
                return (entry.BestMove, entry.Score);
            }
        }
        
        int currentPlayerId = gameState.Players[gameState.CurrentPlayerIndex].Id;
        bool isBotTurn = currentPlayerId == botPlayerId;
        
        // Generate moves with ordering (pawn moves first for better pruning)
        var moves = GenerateOrderedMoves(gameState, currentPlayerId, difficulty);
        
        if (!moves.Any())
        {
            // No legal moves (shouldn't happen in valid game)
            return (null, EvaluatePosition(gameState, botPlayerId, difficulty));
        }
        
        Move? bestMove = null;
        double bestScore = isMaximizing ? double.NegativeInfinity : double.PositiveInfinity;
        
        foreach (var move in moves)
        {
            // Apply move to a copy of the game state
            var newState = CloneGameState(gameState);
            ApplyMove(newState, move);
            
            // Recursive call
            var (_, score) = MinimaxAlphaBeta(newState, botPlayerId, depth - 1, 
                alpha, beta, !isMaximizing, difficulty);
            
            // Maximizing player (bot)
            if (isMaximizing)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, score);
            }
            // Minimizing player (opponent)
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                beta = Math.Min(beta, score);
            }
            
            // Alpha-Beta pruning
            if (beta <= alpha)
                break;
            
            // Early cutoff for decisive positions (Hard mode)
            if (difficulty == BotDifficulty.Hard && Math.Abs(score) > DECISIVE_THRESHOLD)
                break;
        }
        
        // Store in transposition table (Hard mode only)
        if (difficulty == BotDifficulty.Hard && bestMove != null)
        {
            ulong hash = ComputeStateHash(gameState);
            _transpositionTable[hash] = new TranspositionEntry
            {
                BestMove = bestMove,
                Score = bestScore,
                Depth = depth
            };
        }
        
        return (bestMove, bestScore);
    }
    
    #endregion
    
    #region Move Generation
    
    /// <summary>
    /// Generate moves with ordering: pawn moves first, then strategic walls.
    /// </summary>
    private List<Move> GenerateOrderedMoves(GameState gameState, int playerId, BotDifficulty difficulty)
    {
        var moves = new List<Move>();
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return moves;
        
        // 1. Generate pawn moves (always first for better pruning)
        var pawnMoves = GeneratePawnMoves(gameState, playerId);
        moves.AddRange(pawnMoves);
        
        // 2. Generate wall moves (strategically filtered)
        if (player.WallsRemaining > 0)
        {
            var wallMoves = GenerateStrategicWallMoves(gameState, playerId, difficulty);
            moves.AddRange(wallMoves);
        }
        
        return moves;
    }
    
    /// <summary>
    /// Generate all valid pawn moves for the player.
    /// </summary>
    private List<Move> GeneratePawnMoves(GameState gameState, int playerId)
    {
        var moves = new List<Move>();
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return moves;
        
        var validPositions = _validationService.GetValidPawnMoves(gameState, playerId);
        
        foreach (var pos in validPositions)
        {
            moves.Add(new Move
            {
                Type = MoveType.Pawn,
                PlayerId = playerId,
                From = player.Position,
                To = pos,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
        
        return moves;
    }
    
    /// <summary>
    /// Generate strategic wall candidates (NOT all possible walls).
    /// Walls must satisfy at least one strategic criterion.
    /// </summary>
    private List<Move> GenerateStrategicWallMoves(GameState gameState, int playerId, BotDifficulty difficulty)
    {
        var moves = new List<Move>();
        
        // Easy mode: disable or severely limit wall placement
        if (difficulty == BotDifficulty.Easy)
        {
            // Only place walls 20% of the time in easy mode
            if (new Random().NextDouble() > 0.2)
                return moves;
        }
        
        // Get opponent's shortest path
        var opponents = gameState.Players.Where(p => p.Id != playerId).ToList();
        if (!opponents.Any()) return moves;
        
        var primaryOpponent = opponents[0]; // In 2-player, there's only one
        var opponentPath = GetShortestPath(gameState, primaryOpponent.Id);
        
        if (opponentPath == null || opponentPath.Count == 0)
            return moves;
        
        var strategicWalls = new HashSet<Wall>(new WallEqualityComparer());
        
        // Strategy 1: Block opponent's shortest path
        AddPathBlockingWalls(gameState, opponentPath, strategicWalls);
        
        // Strategy 2: Walls adjacent to existing walls (build wall structures)
        if (difficulty != BotDifficulty.Easy)
        {
            AddAdjacentWalls(gameState, strategicWalls);
        }
        
        // Strategy 3 (Medium/Hard): Walls that force opponent detours
        if (difficulty == BotDifficulty.Hard)
        {
            AddDetourForcingWalls(gameState, primaryOpponent, strategicWalls);
        }
        
        // Convert strategic walls to moves, filtering for validity
        foreach (var wall in strategicWalls)
        {
            // Validate the wall placement
            if (!_validationService.IsValidWallPlacement(gameState, wall))
                continue;
            
            // Check that it increases opponent's path length
            var testState = CloneGameState(gameState);
            testState.Walls.Add(wall);
            
            var newOpponentPath = GetShortestPath(testState, primaryOpponent.Id);
            if (newOpponentPath == null || newOpponentPath.Count <= opponentPath.Count)
                continue; // Wall doesn't help, skip it
            
            moves.Add(new Move
            {
                Type = MoveType.Wall,
                PlayerId = playerId,
                Wall = wall,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
            
            // Limit wall candidates to avoid explosion
            if (moves.Count >= 15) // Max 15 wall candidates
                break;
        }
        
        return moves;
    }
    
    /// <summary>
    /// Add walls that block the opponent's shortest path.
    /// </summary>
    private void AddPathBlockingWalls(GameState gameState, List<Position> opponentPath, 
        HashSet<Wall> strategicWalls)
    {
        // For each edge in the path, try to place blocking walls
        for (int i = 0; i < opponentPath.Count - 1; i++)
        {
            var from = opponentPath[i];
            var to = opponentPath[i + 1];
            
            var blockingWalls = GetWallsBlockingEdge(from, to, gameState.BoardSize);
            foreach (var wall in blockingWalls)
            {
                strategicWalls.Add(wall);
            }
        }
    }
    
    /// <summary>
    /// Add walls adjacent to existing walls (build wall structures).
    /// </summary>
    private void AddAdjacentWalls(GameState gameState, HashSet<Wall> strategicWalls)
    {
        foreach (var existingWall in gameState.Walls)
        {
            var adjacentWalls = GetAdjacentWalls(existingWall, gameState.BoardSize);
            foreach (var wall in adjacentWalls)
            {
                strategicWalls.Add(wall);
            }
        }
    }
    
    /// <summary>
    /// Add walls that force the opponent to take detours.
    /// </summary>
    private void AddDetourForcingWalls(GameState gameState, Player opponent, 
        HashSet<Wall> strategicWalls)
    {
        // Place walls near the opponent's current position
        var pos = opponent.Position;
        int boardSize = gameState.BoardSize;
        
        // Try walls around the opponent
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                int row = pos.Row + dr;
                int col = pos.Col + dc;
                
                // Horizontal walls
                if (IsValidWallPosition(row, col, true, boardSize))
                {
                    strategicWalls.Add(new Wall
                    {
                        Position = new Position { Row = row, Col = col },
                        IsHorizontal = true
                    });
                }
                
                // Vertical walls
                if (IsValidWallPosition(row, col, false, boardSize))
                {
                    strategicWalls.Add(new Wall
                    {
                        Position = new Position { Row = row, Col = col },
                        IsHorizontal = false
                    });
                }
            }
        }
    }
    
    /// <summary>
    /// Get walls that would block movement from 'from' to 'to'.
    /// </summary>
    private List<Wall> GetWallsBlockingEdge(Position from, Position to, int boardSize)
    {
        var walls = new List<Wall>();
        int dRow = to.Row - from.Row;
        int dCol = to.Col - from.Col;
        
        // Moving vertically (need horizontal walls)
        if (dCol == 0 && Math.Abs(dRow) == 1)
        {
            int wallRow = Math.Max(from.Row, to.Row);
            int wallCol = from.Col;
            
            // Horizontal wall at (wallRow, wallCol)
            if (IsValidWallPosition(wallRow, wallCol, true, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = wallRow, Col = wallCol },
                    IsHorizontal = true
                });
            }
            
            // Also try adjacent position
            if (IsValidWallPosition(wallRow, wallCol - 1, true, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = wallRow, Col = wallCol - 1 },
                    IsHorizontal = true
                });
            }
        }
        // Moving horizontally (need vertical walls)
        else if (dRow == 0 && Math.Abs(dCol) == 1)
        {
            int wallRow = from.Row;
            int wallCol = Math.Max(from.Col, to.Col);
            
            // Vertical wall at (wallRow, wallCol)
            if (IsValidWallPosition(wallRow, wallCol, false, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = wallRow, Col = wallCol },
                    IsHorizontal = false
                });
            }
            
            // Also try adjacent position
            if (IsValidWallPosition(wallRow - 1, wallCol, false, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = wallRow - 1, Col = wallCol },
                    IsHorizontal = false
                });
            }
        }
        
        return walls;
    }
    
    /// <summary>
    /// Get walls adjacent to an existing wall.
    /// </summary>
    private List<Wall> GetAdjacentWalls(Wall existingWall, int boardSize)
    {
        var walls = new List<Wall>();
        var pos = existingWall.Position;
        
        if (existingWall.IsHorizontal)
        {
            // Adjacent horizontal walls (left/right)
            if (IsValidWallPosition(pos.Row, pos.Col - 1, true, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = pos.Row, Col = pos.Col - 1 },
                    IsHorizontal = true
                });
            }
            if (IsValidWallPosition(pos.Row, pos.Col + 1, true, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = pos.Row, Col = pos.Col + 1 },
                    IsHorizontal = true
                });
            }
        }
        else
        {
            // Adjacent vertical walls (up/down)
            if (IsValidWallPosition(pos.Row - 1, pos.Col, false, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = pos.Row - 1, Col = pos.Col },
                    IsHorizontal = false
                });
            }
            if (IsValidWallPosition(pos.Row + 1, pos.Col, false, boardSize))
            {
                walls.Add(new Wall
                {
                    Position = new Position { Row = pos.Row + 1, Col = pos.Col },
                    IsHorizontal = false
                });
            }
        }
        
        return walls;
    }
    
    /// <summary>
    /// Check if a wall position is within valid board bounds.
    /// Critical: walls at (0,0) are not placeable, vertical walls at column 0 are invalid, etc.
    /// </summary>
    private bool IsValidWallPosition(int row, int col, bool isHorizontal, int boardSize)
    {
        // Wall positions are constrained to prevent out-of-bounds placement
        int maxCoord = boardSize - 2; // Walls can be placed at positions 0 to (boardSize-2)
        
        if (isHorizontal)
        {
            // Horizontal walls: row can be 0 to boardSize-1, col must be 0 to boardSize-2
            // BUT: walls at row 0, col 0 don't make sense in standard Quoridor
            // Let's prevent walls at the very edges that don't block anything
            if (row <= 0 || row >= boardSize) return false;
            if (col < 0 || col > maxCoord) return false;
        }
        else
        {
            // Vertical walls: col can be 0 to boardSize-1, row must be 0 to boardSize-2
            // BUT: vertical walls at column 0 are outside the board
            if (col <= 0 || col >= boardSize) return false;
            if (row < 0 || row > maxCoord) return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Evaluation Function
    
    /// <summary>
    /// Sophisticated evaluation function with multiple weighted components.
    /// </summary>
    private double EvaluatePosition(GameState gameState, int botPlayerId, BotDifficulty difficulty)
    {
        // Check for terminal states
        if (gameState.GameStatus == GameStatus.Finished)
        {
            if (gameState.Winner == botPlayerId)
                return WINNING_SCORE;
            else
                return LOSING_SCORE;
        }
        
        var bot = gameState.Players.FirstOrDefault(p => p.Id == botPlayerId);
        if (bot == null) return 0;
        
        var opponents = gameState.Players.Where(p => p.Id != botPlayerId).ToList();
        if (!opponents.Any()) return 0;
        
        var primaryOpponent = opponents[0]; // Main opponent in 2-player game
        
        // Get shortest paths
        int botMinPath = GetShortestPathLength(gameState, botPlayerId);
        int opponentMinPath = GetShortestPathLength(gameState, primaryOpponent.Id);
        
        // Immediate win/loss detection overrides everything
        if (botMinPath == 0) return WINNING_SCORE;
        if (opponentMinPath == 0) return LOSING_SCORE;
        
        // H1: Path difference (primary heuristic)
        double pathDiff = opponentMinPath - botMinPath;
        
        // H2: Wall difference
        double wallDiff = bot.WallsRemaining - primaryOpponent.WallsRemaining;
        
        // H3: Path flexibility (number of alternative shortest paths)
        double pathFlexibility = GetPathFlexibility(gameState, botPlayerId) - 
                                 GetPathFlexibility(gameState, primaryOpponent.Id);
        
        // H4: Positional advantage
        double positional = EvaluatePositionalAdvantage(gameState, bot, botMinPath) -
                           EvaluatePositionalAdvantage(gameState, primaryOpponent, opponentMinPath);
        
        // H5: Wall efficiency (how much last wall impacted opponent)
        double wallEfficiency = EvaluateWallEfficiency(gameState, botPlayerId);
        
        // Weighted sum
        double score = W1_PATH_DIFF * pathDiff +
                      W2_WALL_DIFF * wallDiff +
                      W3_PATH_FLEXIBILITY * pathFlexibility +
                      W4_POSITIONAL * positional +
                      W5_WALL_EFFICIENCY * wallEfficiency;
        
        // Scale down for easy mode (less aggressive)
        if (difficulty == BotDifficulty.Easy)
        {
            score *= 0.5;
        }
        
        return score;
    }
    
    /// <summary>
    /// Evaluate positional advantage based on game phase.
    /// </summary>
    private double EvaluatePositionalAdvantage(GameState gameState, Player player, int minPath)
    {
        int boardSize = gameState.BoardSize;
        double advantage = 0;
        
        // Early game: prefer centrality
        if (minPath > boardSize / 2)
        {
            int centerRow = boardSize / 2;
            int centerCol = boardSize / 2;
            double distToCenter = Math.Sqrt(
                Math.Pow(player.Position.Row - centerRow, 2) +
                Math.Pow(player.Position.Col - centerCol, 2));
            advantage -= distToCenter * 0.5; // Closer to center is better
        }
        // Late game: prefer proximity to goal
        else
        {
            int distToGoal = Math.Abs(player.Position.Row - player.GoalRow);
            advantage -= distToGoal * 1.0; // Closer to goal is better
        }
        
        return advantage;
    }
    
    /// <summary>
    /// Evaluate wall efficiency (how much walls are helping).
    /// </summary>
    private double EvaluateWallEfficiency(GameState gameState, int playerId)
    {
        // Simple metric: more walls placed = more control
        // This is a placeholder; more sophisticated analysis could track
        // how much each wall increases opponent path length
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return 0;
        
        int wallsPlaced = (gameState.BoardSize == 9 ? 10 : 5) - player.WallsRemaining;
        return wallsPlaced * 0.5;
    }
    
    /// <summary>
    /// Calculate path flexibility (number of alternative shortest paths).
    /// Higher flexibility = more options = better.
    /// </summary>
    private double GetPathFlexibility(GameState gameState, int playerId)
    {
        // Simplified: count the number of valid pawn moves
        // More sophisticated: count alternative shortest paths (computationally expensive)
        var validMoves = _validationService.GetValidPawnMoves(gameState, playerId);
        return validMoves.Count;
    }
    
    #endregion
    
    #region Pathfinding with Caching
    
    /// <summary>
    /// Get shortest path length with caching.
    /// </summary>
    private int GetShortestPathLength(GameState gameState, int playerId)
    {
        string cacheKey = GeneratePathCacheKey(gameState, playerId);
        
        if (_pathCache.TryGetValue(cacheKey, out int cachedLength))
        {
            return cachedLength;
        }
        
        var path = GetShortestPath(gameState, playerId);
        int length = path?.Count ?? int.MaxValue;
        
        _pathCache[cacheKey] = length;
        return length;
    }
    
    /// <summary>
    /// Get shortest path using BFS.
    /// Returns null if no path exists.
    /// </summary>
    private List<Position>? GetShortestPath(GameState gameState, int playerId)
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return null;
        
        var queue = new Queue<(Position pos, List<Position> path)>();
        var visited = new HashSet<Position>(new PositionEqualityComparer());
        
        queue.Enqueue((player.Position, new List<Position> { player.Position }));
        visited.Add(player.Position);
        
        while (queue.Count > 0)
        {
            var (current, path) = queue.Dequeue();
            
            // Check if goal reached
            if (IsGoalPosition(current, player, gameState.BoardSize))
            {
                return path;
            }
            
            // Explore neighbors
            var neighbors = GetValidNeighbors(gameState, current);
            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    var newPath = new List<Position>(path) { neighbor };
                    queue.Enqueue((neighbor, newPath));
                }
            }
        }
        
        return null; // No path found
    }
    
    /// <summary>
    /// Check if a position is the goal for the player.
    /// </summary>
    private bool IsGoalPosition(Position pos, Player player, int boardSize)
    {
        if (player.GoalRow == -1)
        {
            // Horizontal goal
            return pos.Col == 0 || pos.Col == boardSize - 1;
        }
        
        return pos.Row == player.GoalRow;
    }
    
    /// <summary>
    /// Get valid neighboring positions (not blocked by walls).
    /// </summary>
    private List<Position> GetValidNeighbors(GameState gameState, Position pos)
    {
        var neighbors = new List<Position>();
        int boardSize = gameState.BoardSize;
        
        var directions = new[]
        {
            new Position { Row = -1, Col = 0 },  // Up
            new Position { Row = 1, Col = 0 },   // Down
            new Position { Row = 0, Col = -1 },  // Left
            new Position { Row = 0, Col = 1 }    // Right
        };
        
        foreach (var dir in directions)
        {
            var next = new Position
            {
                Row = pos.Row + dir.Row,
                Col = pos.Col + dir.Col
            };
            
            // Check bounds
            if (next.Row < 0 || next.Row >= boardSize || 
                next.Col < 0 || next.Col >= boardSize)
                continue;
            
            // Check if wall blocks this move
            if (IsWallBlockingMove(gameState, pos, next))
                continue;
            
            neighbors.Add(next);
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// Check if a wall blocks movement between two adjacent positions.
    /// </summary>
    private bool IsWallBlockingMove(GameState gameState, Position from, Position to)
    {
        int dRow = to.Row - from.Row;
        int dCol = to.Col - from.Col;
        
        // Must be adjacent orthogonal move
        if (Math.Abs(dRow) + Math.Abs(dCol) != 1)
            return true; // Not a valid single-step move
        
        foreach (var wall in gameState.Walls)
        {
            // Moving vertically (check horizontal walls)
            if (dCol == 0 && wall.IsHorizontal)
            {
                int wallRow = wall.Position.Row;
                int wallCol = wall.Position.Col;
                
                // Moving down: wall blocks if it's between from and to
                if (from.Row < to.Row && wallRow == to.Row &&
                    (wallCol == from.Col || wallCol + 1 == from.Col))
                    return true;
                
                // Moving up: wall blocks if it's between from and to
                if (from.Row > to.Row && wallRow == from.Row &&
                    (wallCol == from.Col || wallCol + 1 == from.Col))
                    return true;
            }
            
            // Moving horizontally (check vertical walls)
            if (dRow == 0 && !wall.IsHorizontal)
            {
                int wallRow = wall.Position.Row;
                int wallCol = wall.Position.Col;
                
                // Moving right: wall blocks if it's between from and to
                if (from.Col < to.Col && wallCol == to.Col &&
                    (wallRow == from.Row || wallRow + 1 == from.Row))
                    return true;
                
                // Moving left: wall blocks if it's between from and to
                if (from.Col > to.Col && wallCol == from.Col &&
                    (wallRow == from.Row || wallRow + 1 == from.Row))
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Generate cache key for path results.
    /// </summary>
    private string GeneratePathCacheKey(GameState gameState, int playerId)
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return "";
        
        // Key: player position + all wall positions
        var wallsKey = string.Join(",", gameState.Walls
            .OrderBy(w => w.Position.Row)
            .ThenBy(w => w.Position.Col)
            .Select(w => $"{w.Position.Row},{w.Position.Col},{(w.IsHorizontal ? 'h' : 'v')}"));
        
        return $"{player.Position.Row},{player.Position.Col}|{wallsKey}";
    }
    
    #endregion
    
    #region Game State Utilities
    
    /// <summary>
    /// Clone game state for simulation (does not mutate original).
    /// </summary>
    private GameState CloneGameState(GameState original)
    {
        return new GameState
        {
            BoardSize = original.BoardSize,
            Players = original.Players.Select(p => new Player
            {
                Id = p.Id,
                Color = p.Color,
                Position = new Position { Row = p.Position.Row, Col = p.Position.Col },
                WallsRemaining = p.WallsRemaining,
                GoalRow = p.GoalRow,
                Name = p.Name,
                UserId = p.UserId,
                Type = p.Type,
                BotDifficulty = p.BotDifficulty
            }).ToList(),
            CurrentPlayerIndex = original.CurrentPlayerIndex,
            Walls = new List<Wall>(original.Walls.Select(w => new Wall
            {
                Position = new Position { Row = w.Position.Row, Col = w.Position.Col },
                IsHorizontal = w.IsHorizontal
            })),
            GameStatus = original.GameStatus,
            Winner = original.Winner,
            MoveHistory = new List<Move>(original.MoveHistory),
            HistoryIndex = original.HistoryIndex
        };
    }
    
    /// <summary>
    /// Apply a move to the game state (mutates state).
    /// </summary>
    private void ApplyMove(GameState gameState, Move move)
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == move.PlayerId);
        if (player == null) return;
        
        if (move.Type == MoveType.Pawn && move.To != null)
        {
            // Move pawn
            player.Position = new Position { Row = move.To.Row, Col = move.To.Col };
            
            // Check for win
            if (_validationService.IsGameWon(gameState, player.Id))
            {
                gameState.GameStatus = GameStatus.Finished;
                gameState.Winner = player.Id;
                return;
            }
        }
        else if (move.Type == MoveType.Wall && move.Wall != null)
        {
            // Place wall
            gameState.Walls.Add(new Wall
            {
                Position = new Position 
                { 
                    Row = move.Wall.Position.Row, 
                    Col = move.Wall.Position.Col 
                },
                IsHorizontal = move.Wall.IsHorizontal
            });
            player.WallsRemaining--;
        }
        
        // Move to next player
        gameState.CurrentPlayerIndex = (gameState.CurrentPlayerIndex + 1) % gameState.Players.Count;
    }
    
    /// <summary>
    /// Compute Zobrist-style hash for transposition table.
    /// </summary>
    private ulong ComputeStateHash(GameState gameState)
    {
        ulong hash = 0;
        
        // Hash player positions
        foreach (var player in gameState.Players)
        {
            hash ^= (ulong)((player.Position.Row * 1000 + player.Position.Col) * (player.Id + 1));
        }
        
        // Hash walls
        foreach (var wall in gameState.Walls)
        {
            hash ^= (ulong)((wall.Position.Row * 10000 + wall.Position.Col * 100 + 
                           (wall.IsHorizontal ? 1 : 0)) * 31);
        }
        
        // Hash current player
        hash ^= (ulong)(gameState.CurrentPlayerIndex * 97);
        
        return hash;
    }
    
    #endregion
    
    #region Helper Classes
    
    private class TranspositionEntry
    {
        public Move? BestMove { get; set; }
        public double Score { get; set; }
        public int Depth { get; set; }
    }
    
    private class WallEqualityComparer : IEqualityComparer<Wall>
    {
        public bool Equals(Wall? x, Wall? y)
        {
            if (x == null || y == null) return x == y;
            return x.Position.Row == y.Position.Row &&
                   x.Position.Col == y.Position.Col &&
                   x.IsHorizontal == y.IsHorizontal;
        }
        
        public int GetHashCode(Wall obj)
        {
            return HashCode.Combine(obj.Position.Row, obj.Position.Col, obj.IsHorizontal);
        }
    }
    
    private class PositionEqualityComparer : IEqualityComparer<Position>
    {
        public bool Equals(Position? x, Position? y)
        {
            if (x == null || y == null) return x == y;
            return x.Row == y.Row && x.Col == y.Col;
        }
        
        public int GetHashCode(Position obj)
        {
            return HashCode.Combine(obj.Row, obj.Col);
        }
    }
    
    #endregion
}
