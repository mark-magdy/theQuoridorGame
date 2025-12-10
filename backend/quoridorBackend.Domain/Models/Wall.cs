namespace QuoridorBackend.Domain.Models;

public class Wall
{
    public Position Position { get; set; } = new(); // Top-left corner
    public bool IsHorizontal { get; set; }

    /// <summary>
    /// Get wall ID in algebraic notation (e.g., "a1h" for horizontal, "a1v" for vertical)
    /// </summary>
    public string GetWallId()
    {
        return $"{Position.ToAlgebraicNotation()}{(IsHorizontal ? "h" : "v")}";
    }

    /// <summary>
    /// Create wall from algebraic notation (e.g., "a1h", "b2v")
    /// </summary>
    public static Wall FromWallId(string wallId)
    {
        if (string.IsNullOrEmpty(wallId) || wallId.Length < 3)
            throw new ArgumentException("Invalid wall ID");

        char orientation = wallId[wallId.Length - 1];
        string positionNotation = wallId.Substring(0, wallId.Length - 1);

        return new Wall
        {
            Position = Position.FromAlgebraicNotation(positionNotation),
            IsHorizontal = orientation == 'h'
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is Wall other)
            return Position.Equals(other.Position) && IsHorizontal == other.IsHorizontal;
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Position, IsHorizontal);
    
    public override string ToString() => GetWallId();
}
