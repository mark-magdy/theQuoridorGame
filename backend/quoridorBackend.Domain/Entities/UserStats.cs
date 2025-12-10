using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.Entities;

public class UserStats
{
    [Key]
    public Guid UserId { get; set; }
    
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalMoves { get; set; }
    public int WallsPlaced { get; set; }
    public int? FastestWin { get; set; } // Moves to win
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User User { get; set; } = null!;
}
