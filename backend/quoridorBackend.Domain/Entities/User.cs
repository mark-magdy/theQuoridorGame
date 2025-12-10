using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public UserStats? Stats { get; set; }
    public ICollection<Game> CreatedGames { get; set; } = new List<Game>();
    public ICollection<GamePlayer> GameParticipations { get; set; } = new List<GamePlayer>();
}
