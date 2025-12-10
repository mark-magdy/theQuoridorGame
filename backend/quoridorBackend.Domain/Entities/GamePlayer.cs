using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.Entities;

public class GamePlayer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public int PlayerId { get; set; } // 0-3
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    
    // Navigation
    public Game Game { get; set; } = null!;
    public User User { get; set; } = null!;
}
