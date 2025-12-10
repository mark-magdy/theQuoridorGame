using QuoridorBackend.Domain.Enums;

namespace QuoridorBackend.Domain.Models;

public class Move
{
    public MoveType Type { get; set; }
    public int PlayerId { get; set; }
    public long Timestamp { get; set; }
    
    // For pawn moves
    public Position? From { get; set; }
    public Position? To { get; set; }
    
    // For wall placement
    public Wall? Wall { get; set; }
}
