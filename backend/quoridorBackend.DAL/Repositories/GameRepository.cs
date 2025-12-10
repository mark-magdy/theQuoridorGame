using Microsoft.EntityFrameworkCore;
using QuoridorBackend.DAL.Data;
using QuoridorBackend.DAL.Repositories.Interfaces;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.DAL.Repositories;

public class GameRepository : Repository<Game>, IGameRepository
{
    public GameRepository(QuoridorDbContext context) : base(context)
    {
    }

    public async Task<Game?> GetByJoinCodeAsync(string joinCode)
    {
        return await _dbSet.FirstOrDefaultAsync(g => g.JoinCode == joinCode);
    }

    public async Task<Game?> GetWithPlayersAsync(Guid gameId)
    {
        return await _dbSet
            .Include(g => g.GamePlayers)
            .ThenInclude(gp => gp.User)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<Game?> GetWithMovesAsync(Guid gameId)
    {
        return await _dbSet
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<IEnumerable<Game>> GetActiveGamesAsync()
    {
        return await _dbSet
            .Where(g => g.Status == "waiting" || g.Status == "playing")
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetGamesWithFiltersAsync(
        string? status, 
        int? playerCount, 
        int limit, 
        int offset)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(g => g.Status == status);
        }

        // PlayerCount filtering would need to be done after deserialization
        // or with a computed column - leaving as placeholder

        return await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }
}
