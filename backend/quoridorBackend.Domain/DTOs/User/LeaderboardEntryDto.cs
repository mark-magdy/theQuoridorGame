namespace QuoridorBackend.Domain.DTOs.User;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int GamesWon { get; set; }
    public int GamesPlayed { get; set; }
    public double WinRate { get; set; }
}
