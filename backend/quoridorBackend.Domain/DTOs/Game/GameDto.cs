using QuoridorBackend.Domain.Models;

namespace QuoridorBackend.Domain.DTOs.Game;

public class GameDto
{
    public Guid Id { get; set; }
    public GameState GameState { get; set; } = null!;
    public GameSettings Settings { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public bool IsPrivate { get; set; }
    public List<string> ConnectedPlayers { get; set; } = new();
}
