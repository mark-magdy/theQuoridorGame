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
     int size = gameState.BoardSize;

    //Board bounds check (MUST be first)
    if (to.Row < 0 || to.Row >= size ||
        to.Col < 0 || to.Col >= size)
        return false;
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

    
public bool IsValidWallPlacement(GameState gameState, Wall wall)
{
    // Player & turn validation
    var player = gameState.Players.FirstOrDefault(p => p.Id == gameState.CurrentPlayerIndex);
    if (player == null)
        return false;

    if (player.WallsRemaining <= 0)
        return false;

    //Bounds check
    if (!IsWallWithinBounds(gameState.BoardSize, wall))
        return false;

    //Overlap / crossing check
    if (DoesWallOverlap(gameState.Walls, wall))
        return false;

    // Path existence check (BFS)
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
public List<Wall> GetValidWallPlacements(GameState gameState, int playerId)
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
            var horizontalWall = new Wall
            {
                Position = new Position { Row = row, Col = col },
                IsHorizontal = true
            };

            if (IsValidWallPlacement(gameState, horizontalWall))
                validWalls.Add(horizontalWall);

            // Try vertical wall
            var verticalWall = new Wall
            {
                Position = new Position { Row = row, Col = col },
                IsHorizontal = false
            };

            if (IsValidWallPlacement(gameState, verticalWall))
                validWalls.Add(verticalWall);
        }
    }

    return validWalls;
}

// A* to find a path to the goal to validate the placement of the wall
public bool HasPathToGoal(GameState gameState, int playerId)
{
    // Find the player by ID
    var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
    if (player == null)
        return false;

    var openSet = new PriorityQueue<Position, int>();
    var gScore = new Dictionary<Position, int>();
    var visited = new HashSet<Position>();

    Position start = player.Position;

    gScore[start] = 0;
    openSet.Enqueue(start, Heuristic(player, start));

    while (openSet.Count > 0)
    {
        var current = openSet.Dequeue();

        if (IsGoalReached(gameState,player, current))
            return true;

        if (visited.Contains(current))
            continue;

        visited.Add(current);

        foreach (var neighbor in GetNeighbors(gameState, current))
        {
            int Gn = gScore[current] + 1;

            if (!gScore.ContainsKey(neighbor) || Gn < gScore[neighbor])
            {
                gScore[neighbor] = Gn;
                int fScore = Gn + Heuristic(player, neighbor);
                openSet.Enqueue(neighbor, fScore);
            }
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

    
// first helper function
  private bool IsWallWithinBounds(int boardSize, Wall wall)
{
    // Limits the coordinate to (Size - 2)
    int maxLength = boardSize - 2; 

    // Limits the coordinate to (Size - 1)
    int maxGap = boardSize - 1; 

    if (wall.IsHorizontal) // Horizontal
    {
        return wall.Position.Col >= 0 && wall.Position.Col <= maxLength &&
               wall.Position.Row >= 0 && wall.Position.Row <= maxGap;
    }
    else // Vertical
    {
        return wall.Position.Row >= 0 && wall.Position.Row <= maxLength &&
               wall.Position.Col >= 0 && wall.Position.Col <= maxGap;
    }
}   
    
// second helper function
    private bool DoesWallOverlap(List<Wall> existingWalls, Wall newWall)
    {
    foreach (var wall in existingWalls)
    {
        //overlap
        if (wall.Equals(newWall))
            return true;

        // Crossing
        if (wall.IsHorizontal != newWall.IsHorizontal)
        {
            if (DoWallsCross(wall, newWall))
                return true;
        }

        //Parallel adjacent walls
        if (wall.IsHorizontal == newWall.IsHorizontal)
        {
            // Horizontal touching end-to-end
            if (wall.IsHorizontal &&
                wall.Position.Row == newWall.Position.Row &&
                Math.Abs(wall.Position.Col - newWall.Position.Col) == 1)
                return true;

            // Vertical touching end-to-end
            if (!wall.IsHorizontal &&
                wall.Position.Col == newWall.Position.Col &&
                Math.Abs(wall.Position.Row - newWall.Position.Row) == 1)
                return true;
        }
    }

    return false;
}
    // used to check the crossing called by -----> DoesWallOverlap()
    private bool DoWallsCross(Wall w1, Wall w2)
    {
    // Ensure w1 is horizontal, w2 vertical
    if (!w1.IsHorizontal)
        (w1, w2) = (w2, w1);
    return w1.Position.Row == w2.Position.Row +1 && w1.Position.Col + 1 == w2.Position.Col;
    }


// called inside the BFS
    private bool IsGoalReached(GameState gameState, Player player, Position pos)
    {
    return pos.Row == player.GoalRow;
    }

//used to return the valid moves available to the pawn after placing a wall
// used inside the pawn move and BFS while checking for a goal after wall placement
    private IEnumerable<Position> GetNeighbors(GameState gameState, Position pos)
    {
    int size = gameState.BoardSize;

    var directions = new[]
         {
        new Position { Row = -1, Col = 0 },
        new Position { Row = 1,  Col = 0 },
        new Position { Row = 0,  Col = -1 },
        new Position { Row = 0,  Col = 1 }
        };

    foreach (var d in directions)
        {
        var next = new Position
        {
            Row = pos.Row + d.Row,
            Col = pos.Col + d.Col
        };

        if (next.Row < 0 || next.Row >= size ||
            next.Col < 0 || next.Col >= size)
            continue;

        if (IsWallBlocking(gameState, pos, next))
            continue;

        yield return next;
     }
    }
   
    private bool IsWallBlocking(GameState gameState, Position from, Position to)
    {
    int dRow = to.Row - from.Row;
    int dCol = to.Col - from.Col;

    // adjacent orthogonal moves are valid
    if (Math.Abs(dRow) + Math.Abs(dCol) != 1)
        return false;

    foreach (var wall in gameState.Walls)
    {
        // // Moving vertically (check horizontal walls)
        // if (dCol == 0 && wall.IsHorizontal)
        // {
        //     int wallRow = Math.Min(from.Row, to.Row);

        //     if (wall.Position.Row+1 == wallRow &&
        //         (wall.Position.Col == from.Col ||
        //          wall.Position.Col+1 == from.Col ))
        //         return true;
        // }

        // // Moving horizontally (check vertical walls)
        // if (dRow == 0 && !wall.IsHorizontal)
        // {
        //     int wallCol = Math.Min(from.Col, to.Col);

        //     if (wall.Position.Col+1 == wallCol &&
        //         (wall.Position.Row == from.Row ||
        //          wall.Position.Row +1 == from.Row ))
        //         return true;
        // }
        if (wall.IsHorizontal && dCol == 0) 
        {
            int wallRow = wall.Position.Row;
            int wallCol = wall.Position.Col;

            // Moving UP 
            if (from.Row == wallRow && to.Row == wallRow - 1 &&
                (from.Col == wallCol || from.Col == wallCol + 1))
                return true;

            // Moving DOWN 
            if (from.Row == wallRow - 1 && to.Row == wallRow &&
                (from.Col == wallCol || from.Col == wallCol + 1))
                return true;
        }

      if (!wall.IsHorizontal && dRow == 0) 
        {
            int wallRow = wall.Position.Row;
            int wallCol = wall.Position.Col;

            // Moving LEFT 
            if (from.Col == wallCol && to.Col == wallCol - 1 &&
                (from.Row == wallRow || from.Row == wallRow + 1))
                return true;

            // Moving RIGHT 
            if (from.Col == wallCol - 1 && to.Col == wallCol &&
                (from.Row == wallRow || from.Row == wallRow + 1))
                return true;
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
    
    private int Heuristic(Player player, Position pos)
    {
    return Math.Abs(pos.Row - player.GoalRow);
    }


    #endregion
}
