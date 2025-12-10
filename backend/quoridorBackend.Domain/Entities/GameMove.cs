using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.Entities;

public class GameMove
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid GameId { get; set; }
    public int PlayerId { get; set; }
    
    [Required, MaxLength(10)]
    public string MoveType { get; set; } = string.Empty; // "pawn" or "wall"
    
    [Required]
    public string MoveDataJson { get; set; } = string.Empty; // JSON serialized Move
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int MoveNumber { get; set; }
    
    // Navigation
    public Game Game { get; set; } = null!;
}
