using QuoridorBackend.Domain.DTOs.Game;
using QuoridorBackend.Domain.Models;

namespace QuoridorBackend.BLL.Services.Interfaces;

public interface IGameService
{
    Task<CreateGameResponse> CreateBotGameAsync(Guid userId, CreateBotGameRequest request);
    Task<CreateGameResponse> CreateMultiplayerGameAsync(List<Guid> playerUserIds, GameSettings settings);
    Task<GameDto?> GetGameAsync(Guid gameId, Guid userId);
    Task<MakeMoveResponse> MakeMoveAsync(Guid gameId, Guid userId, MakeMoveRequest request);
    Task<AvailableMovesResponse> GetAvailableMovesAsync(Guid gameId, Guid userId);
    Task<bool> DeleteGameAsync(Guid gameId, Guid userId);
    Task<IEnumerable<GameDto>> GetUserGamesAsync(Guid userId);
    Task<IEnumerable<GameDto>> GetUserFinishedGamesAsync(Guid userId);
}
