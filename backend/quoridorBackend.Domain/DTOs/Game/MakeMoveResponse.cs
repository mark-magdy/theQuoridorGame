using QuoridorBackend.Domain.Models;

namespace QuoridorBackend.Domain.DTOs.Game;

public class MakeMoveResponse
{
    public GameState GameState { get; set; } = null!;
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public Move? BotMove { get; set; } // If bot made a move after player
    public bool GameEnded { get; set; }
    public int? WinnerId { get; set; }
}
