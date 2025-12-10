using QuoridorBackend.Domain.Models;

namespace QuoridorBackend.BLL.Services.Interfaces;

public interface IGameValidationService
{
    bool IsValidPawnMove(GameState gameState, int playerId, Position to);
    bool IsValidWallPlacement(GameState gameState, Wall wall);
    List<Position> GetValidPawnMoves(GameState gameState, int playerId);
    List<Wall> GetValidWallPlacements(GameState gameState, int playerId);
    bool HasPathToGoal(GameState gameState, int playerId);
    bool IsGameWon(GameState gameState, int playerId);
}
