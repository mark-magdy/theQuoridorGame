namespace QuoridorBackend.Domain.DTOs.Game;

public class ListGamesResponse
{
    public List<GameDto> Games { get; set; } = new();
    public int TotalCount { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}
