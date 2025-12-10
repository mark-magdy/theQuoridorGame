using QuoridorBackend.Domain.Enums;

namespace QuoridorBackend.Domain.Models;

public class Player
{
    public int Id { get; set; }
    public PlayerColor Color { get; set; }
    public Position Position { get; set; } = new();
    public int WallsRemaining { get; set; }
    public int GoalRow { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public PlayerType Type { get; set; } = PlayerType.Human;
    public BotDifficulty? BotDifficulty { get; set; }
}
