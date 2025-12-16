using QuoridorBackend.Domain.Enums;
using QuoridorBackend.Domain.Models;

namespace QuoridorBackend.BLL.Services.Interfaces;

/// <summary>
/// Quoridor AI bot engine using Minimax with Alpha-Beta pruning.
/// </summary>
public interface IBotEngine
{
    /// <summary>
    /// Get the best move for the bot at the specified difficulty level.
    /// </summary>
    /// <param name="gameState">Current game state (will not be mutated)</param>
    /// <param name="botPlayerId">The bot's player ID</param>
    /// <param name="difficulty">Difficulty level (Easy/Medium/Hard)</param>
    /// <returns>The best move, or null if no legal moves</returns>
    Move? GetBestMove(GameState gameState, int botPlayerId, BotDifficulty difficulty);
}
