namespace QuoridorBackend.Domain.DTOs.Game;

public class JoinGameResponse
{
    public GameDto Game { get; set; } = null!;
    public int PlayerId { get; set; }
}
