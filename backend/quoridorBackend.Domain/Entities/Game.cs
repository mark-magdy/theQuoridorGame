using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.Entities;

public class Game
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string GameStateJson { get; set; } = string.Empty; // JSON serialized GameState
    
    [Required]
    public string SettingsJson { get; set; } = string.Empty; // JSON serialized GameSettings
    
    [Required, MaxLength(20)]
    public string Status { get; set; } = "waiting"; // waiting, playing, paused, finished
    
    public Guid CreatedBy { get; set; }
    public bool IsPrivate { get; set; }
    
    [MaxLength(8)]
    public string? JoinCode { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    
    // Navigation
    public User Creator { get; set; } = null!;
    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
    public ICollection<GameMove> Moves { get; set; } = new List<GameMove>();
}
