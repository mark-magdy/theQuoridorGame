using Microsoft.EntityFrameworkCore;
using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.DAL.Repositories.Interfaces;
using QuoridorBackend.Domain.DTOs.User;

namespace QuoridorBackend.BLL.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetWithStatsAsync(userId);
        
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Stats = user.Stats != null ? new UserStatsDto
            {
                GamesPlayed = user.Stats.GamesPlayed,
                GamesWon = user.Stats.GamesWon,
                TotalMoves = user.Stats.TotalMoves,
                WallsPlaced = user.Stats.WallsPlaced,
                FastestWin = user.Stats.FastestWin
            } : null
        };
    }

    public async Task<UserStatsDto> GetStatsAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetWithStatsAsync(userId);
        
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (user.Stats == null)
        {
            return new UserStatsDto();
        }

        return new UserStatsDto
        {
            GamesPlayed = user.Stats.GamesPlayed,
            GamesWon = user.Stats.GamesWon,
            TotalMoves = user.Stats.TotalMoves,
            WallsPlaced = user.Stats.WallsPlaced,
            FastestWin = user.Stats.FastestWin
        };
    }

    public async Task<LeaderboardDto> GetLeaderboardAsync(int limit = 50, int offset = 0)
    {
        var query = _unitOfWork.Users.GetAllQueryable()
            .Include(u => u.Stats)
            .Where(u => u.Stats != null && u.Stats.GamesPlayed > 0)
            .OrderByDescending(u => u.Stats!.GamesWon)
            .ThenByDescending(u => u.Stats!.GamesPlayed > 0 ? (double)u.Stats.GamesWon / u.Stats.GamesPlayed : 0);

        var totalCount = await query.CountAsync();
        
        var users = await query
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        var entries = users.Select((user, index) => new LeaderboardEntryDto
        {
            Rank = offset + index + 1,
            UserId = user.Id,
            Username = user.Username,
            GamesWon = user.Stats!.GamesWon,
            GamesPlayed = user.Stats.GamesPlayed,
            WinRate = user.Stats.GamesPlayed > 0 ? (double)user.Stats.GamesWon / user.Stats.GamesPlayed * 100 : 0
        }).ToList();

        return new LeaderboardDto
        {
            Entries = entries,
            TotalCount = totalCount,
            Offset = offset,
            Limit = limit
        };
    }
}
