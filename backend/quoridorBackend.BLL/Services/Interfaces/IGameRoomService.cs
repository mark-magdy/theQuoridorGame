using QuoridorBackend.Domain.DTOs;
using QuoridorBackend.Domain.DTOs.Game;

namespace QuoridorBackend.BLL.Services.Interfaces
{
    public interface IGameRoomService
    {
        Task<RoomDto> CreateRoom(string hostUserId, string connectionId, int maxPlayers);
        Task<RoomDto> JoinRoom(string roomId, string userId, string connectionId);
        Task<RoomDto?> LeaveRoom(string roomId, string userId);
        Task<GameDto> StartGame(string roomId, string userId);
        Task<GameDto> MakeMove(GameMoveDto moveDto, string userId);
        Task HandlePlayerDisconnect(string userId, string connectionId);
        Task<RoomDto?> GetRoom(string roomId);
    }
}