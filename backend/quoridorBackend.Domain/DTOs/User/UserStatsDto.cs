namespace QuoridorBackend.Domain.DTOs.User;

public class UserStatsDto
{
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalMoves { get; set; }
    public int WallsPlaced { get; set; }
    public int? FastestWin { get; set; }
    public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;
}
