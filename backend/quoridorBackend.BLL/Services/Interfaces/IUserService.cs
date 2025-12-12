using QuoridorBackend.Domain.DTOs.User;

namespace QuoridorBackend.BLL.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserStatsDto> GetStatsAsync(Guid userId);
    Task<LeaderboardDto> GetLeaderboardAsync(int limit = 50, int offset = 0);
    Task UpdateUserStatsAfterGameAsync(Guid userId, bool won, int moves, int wallsPlaced);
    Task RevertUserStatsAfterGameDeletionAsync(Guid userId, bool won, int moves, int wallsPlaced);
}
