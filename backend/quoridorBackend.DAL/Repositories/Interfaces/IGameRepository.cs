using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Repositories.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    Task<Game?> GetByJoinCodeAsync(string joinCode);
    Task<Game?> GetWithPlayersAsync(Guid gameId);
    Task<Game?> GetWithMovesAsync(Guid gameId);
    Task<IEnumerable<Game>> GetActiveGamesAsync();
    Task<IEnumerable<Game>> GetGamesWithFiltersAsync(string? status, int? playerCount, int limit, int offset);
}
