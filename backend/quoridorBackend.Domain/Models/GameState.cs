using QuoridorBackend.Domain.Enums;

namespace QuoridorBackend.Domain.Models;

public class GameState
{
    public int BoardSize { get; set; }
    public List<Player> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public List<Wall> Walls { get; set; } = new();
    public GameStatus GameStatus { get; set; }
    public int? Winner { get; set; }
    public List<Move> MoveHistory { get; set; } = new();
    public int HistoryIndex { get; set; }
}
