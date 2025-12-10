using QuoridorBackend.Domain.Models;

namespace QuoridorBackend.Domain.DTOs.Game;

public class CreateGameResponse
{
    public Guid GameId { get; set; }
    public GameState GameState { get; set; } = null!;
    public string? JoinCode { get; set; }
}
