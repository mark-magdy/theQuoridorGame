namespace QuoridorBackend.Domain.Models;

public class Position
{
    public int Row { get; set; }
    public int Col { get; set; }

    /// <summary>
    /// Get algebraic notation (e.g., "a1", "b5")
    /// Row 0 = '1', Col 0 = 'a'
    /// </summary>
    public string ToAlgebraicNotation()
    {
        char col = (char)('a' + Col);
        int row = Row + 1;
        return $"{col}{row}";
    }

    /// <summary>
    /// Create position from algebraic notation (e.g., "a1", "b5")
    /// </summary>
    public static Position FromAlgebraicNotation(string notation)
    {
        if (string.IsNullOrEmpty(notation) || notation.Length < 2)
            throw new ArgumentException("Invalid algebraic notation");

        char colChar = char.ToLower(notation[0]);
        int col = colChar - 'a';
        int row = int.Parse(notation.Substring(1)) - 1;

        return new Position { Row = row, Col = col };
    }

    public override bool Equals(object? obj)
    {
        if (obj is Position other)
            return Row == other.Row && Col == other.Col;
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Row, Col);
    
    public override string ToString() => ToAlgebraicNotation();
}
