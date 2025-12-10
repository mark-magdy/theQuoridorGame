namespace QuoridorBackend.Domain.DTOs.Game;

public class AvailableMovesResponse
{
    public List<string> ValidPawnMoves { get; set; } = new(); // Cell IDs in algebraic notation
    public List<string> ValidWallPlacements { get; set; } = new(); // Wall IDs in algebraic notation
}
