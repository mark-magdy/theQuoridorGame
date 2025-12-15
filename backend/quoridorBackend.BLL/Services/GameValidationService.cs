using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.Domain.Models;

namespace QuoridorBackend.BLL.Services;

public class GameValidationService : IGameValidationService
{
    public bool IsValidPawnMove(GameState gameState, int playerId, Position to) // john 
    {
    var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
    Position from = player.Position;
    int boardSize = gameState.BoardSize;
    var opponent = gameState.Players.First(p => p.Id != playerId);
    Position opponentPos = opponent.Position;

    //Cannot move onto another pawn
        if (opponentPos.Row == to.Row &&
            opponentPos.Col == to.Col)
        {
            return false;
        }

    int dr = to.Row - from.Row;
    int dc = to.Col - from.Col;
    int manhattan = Math.Abs(dr) + Math.Abs(dc);

    //Normal move (1 square)
    if (manhattan == 1)
    {
        if (IsWallBlocking(gameState, from, to))
            return false;
        return true;
    }

    //Jump over opponent
    if (manhattan == 2 && (dr == 0 || dc == 0))
    {
        Position middle = new Position
        {
            Row = from.Row + dr / 2,
            Col = from.Col + dc / 2
        };

        if (opponentPos.Row != middle.Row ||
            opponentPos.Col != middle.Col) //the opponent is not in the middle
            return false;

        // Check wall between from → opponent
        if (IsWallBlocking(gameState, from, middle))
            return false;

        // Check wall between opponent → to
        if (IsWallBlocking(gameState,middle, to))
            return false;

        return true;
    }

    // Diagonal side-step (when jump is blocked)
    if (Math.Abs(dr) == 1 && Math.Abs(dc) == 1) //we must ensure straight jum is blocked
    {
        // Must be adjacent to opponent
        if (Math.Abs(opponentPos.Row - from.Row) + Math.Abs(opponentPos.Col - from.Col) != 1)
            return false;

        // Jump forward is blocked
        Position jump = new Position
        {
            Row = opponentPos.Row + (opponentPos.Row - from.Row),
            Col = opponentPos.Col + (opponentPos.Col - from.Col)
        };

        if (IsWallBlocking(gameState, opponentPos, jump))
        {
            // Side move must not be blocked
            if (IsWallBlocking(gameState,from, to))
                return false;

            return true;
        }
    }

    return false;
}

    public bool IsValidWallPlacement(GameState gameState, Wall wall) // zak 
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == gameState.CurrentPlayerIndex);
        if (player == null || player.WallsRemaining <= 0)
            return false;

        // Check if wall is within bounds
        int maxPos = gameState.BoardSize - 2; // Walls can't be placed on the edge
        if (wall.Position.Row < 0 || wall.Position.Row > maxPos ||
            wall.Position.Col < 0 || wall.Position.Col > maxPos)
            return false;

        // Check if wall overlaps with existing walls
        if (gameState.Walls.Any(w => WallsOverlap(w, wall)))
            return false;

        // Temporarily add wall and check if all players can still reach their goals
        gameState.Walls.Add(wall);
        bool allPlayersHavePath = gameState.Players.All(p => HasPathToGoal(gameState, p.Id));
        gameState.Walls.RemoveAt(gameState.Walls.Count - 1);

        return allPlayersHavePath;
    }

    public List<Position> GetValidPawnMoves(GameState gameState, int playerId) // zak 
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return new List<Position>();

        var from = player.Position;
        var validMoves = new List<Position>();

        var deltas = new (int dr, int dc)[]
        {
            // Normal moves
            (-1, 0), (1, 0), (0, -1), (0, 1),

            // Jump moves
            (-2, 0), (2, 0), (0, -2), (0, 2),

            // Diagonal moves
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        foreach (var (dr, dc) in deltas)
        {
            var to = new Position
            {
                Row = from.Row + dr,
                Col = from.Col + dc
            };

            if (IsValidPawnMove(gameState, playerId, to))
                validMoves.Add(to);
        }
        return validMoves;
    }

    public List<Wall> GetValidWallPlacements(GameState gameState, int playerId) // john 
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null || player.WallsRemaining <= 0)
            return new List<Wall>();

        var validWalls = new List<Wall>();
        int maxPos = gameState.BoardSize - 2;

        for (int row = 0; row <= maxPos; row++)
        {
            for (int col = 0; col <= maxPos; col++)
            {
                // Try horizontal wall
                var hWall = new Wall
                {
                    Position = new Position { Row = row, Col = col },
                    IsHorizontal = true
                };
                if (IsValidWallPlacement(gameState, hWall))
                    validWalls.Add(hWall);

                // Try vertical wall
                var vWall = new Wall
                {
                    Position = new Position { Row = row, Col = col },
                    IsHorizontal = false
                };
                if (IsValidWallPlacement(gameState, vWall))
                    validWalls.Add(vWall);
            }
        }

        return validWalls;
    }

    public bool HasPathToGoal(GameState gameState, int playerId) // newcomer 
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return false;

        // BFS to find path to goal
        var queue = new Queue<Position>();
        var visited = new HashSet<string>();
        
        queue.Enqueue(player.Position);
        visited.Add($"{player.Position.Row},{player.Position.Col}");

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Check if reached goal
            if (player.GoalRow == -1) // Horizontal goal (4-player game)
            {
                if (current.Col == 0 || current.Col == gameState.BoardSize - 1)
                    return true;
            }
            else // Vertical goal
            {
                if (current.Row == player.GoalRow)
                    return true;
            }

            // Explore neighbors
            var directions = new[]
            {
                new { Row = -1, Col = 0 },
                new { Row = 1, Col = 0 },
                new { Row = 0, Col = -1 },
                new { Row = 0, Col = 1 }
            };

            foreach (var dir in directions)
            {
                var next = new Position { Row = current.Row + dir.Row, Col = current.Col + dir.Col };
                var key = $"{next.Row},{next.Col}";

                if (next.Row < 0 || next.Row >= gameState.BoardSize ||
                    next.Col < 0 || next.Col >= gameState.BoardSize ||
                    visited.Contains(key) ||
                    IsWallBlocking(gameState, current, next))
                    continue;

                queue.Enqueue(next);
                visited.Add(key);
            }
        }

        return false;
    }

    public bool IsGameWon(GameState gameState, int playerId) // newcomer 
    {
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
            return false;

        if (player.GoalRow == -1) // Horizontal goal
        {
            return player.Position.Col == 0 || player.Position.Col == gameState.BoardSize - 1;
        }
        
        return player.Position.Row == player.GoalRow;
    }

    #region Private Helper Methods

    private bool IsWallBlocking(GameState gameState, Position from, Position to) 
    {
        // Check if a wall blocks movement from 'from' to 'to'
        foreach (var wall in gameState.Walls)
        {
            if (wall.IsHorizontal)
            {
                // Horizontal wall blocks vertical movement
                if (from.Col == to.Col && from.Row != to.Row)
                {
                    int minRow = Math.Min(from.Row, to.Row);
                    int maxRow = Math.Max(from.Row, to.Row);
                    
                    // Wall blocks if it's between the rows and at the same column
                    if (wall.Position.Row == minRow && 
                        (wall.Position.Col == from.Col || wall.Position.Col == from.Col - 1))
                        return true;
                }
            }
            else
            {
                // Vertical wall blocks horizontal movement
                if (from.Row == to.Row && from.Col != to.Col)
                {
                    int minCol = Math.Min(from.Col, to.Col);
                    int maxCol = Math.Max(from.Col, to.Col);
                    
                    // Wall blocks if it's between the columns and at the same row
                    if (wall.Position.Col == minCol && 
                        (wall.Position.Row == from.Row || wall.Position.Row == from.Row - 1))
                        return true;
                }
            }
        }

        return false;
    }

    private bool WallsOverlap(Wall wall1, Wall wall2)
    {
        // Same position and orientation
        if (wall1.Position.Equals(wall2.Position) && wall1.IsHorizontal == wall2.IsHorizontal)
            return true;

        // Walls intersect at center point
        if (wall1.IsHorizontal != wall2.IsHorizontal)
        {
            // Horizontal wall at (r, c) spans columns c and c+1
            // Vertical wall at (r, c) spans rows r and r+1
            if (wall1.IsHorizontal)
            {
                return wall1.Position.Row == wall2.Position.Row &&
                       (wall1.Position.Col == wall2.Position.Col || wall1.Position.Col == wall2.Position.Col + 1);
            }
            else
            {
                return wall2.Position.Row == wall1.Position.Row &&
                       (wall2.Position.Col == wall1.Position.Col || wall2.Position.Col == wall1.Position.Col + 1);
            }
        }

        return false;
    }

    private void AddDiagonalJumps(GameState gameState, Position from, Position occupied, List<Position> validMoves)
    {
        // Calculate the direction we were moving
        int rowDir = occupied.Row - from.Row;
        int colDir = occupied.Col - from.Col;

        // Try perpendicular directions
        var perpDirs = new[]
        {
            new { Row = colDir, Col = rowDir },   // Perpendicular 1
            new { Row = -colDir, Col = -rowDir }  // Perpendicular 2
        };

        foreach (var dir in perpDirs)
        {
            var diagPos = new Position { Row = occupied.Row + dir.Row, Col = occupied.Col + dir.Col };
            
            if (diagPos.Row >= 0 && diagPos.Row < gameState.BoardSize &&
                diagPos.Col >= 0 && diagPos.Col < gameState.BoardSize &&
                !IsWallBlocking(gameState, occupied, diagPos) &&
                !gameState.Players.Any(p => p.Position.Equals(diagPos)))
            {
                validMoves.Add(diagPos);
            }
        }
    }

    #endregion
}
