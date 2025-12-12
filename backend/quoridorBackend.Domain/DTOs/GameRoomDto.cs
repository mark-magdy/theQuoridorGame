namespace QuoridorBackend.Domain.DTOs
{
    public class CreateRoomDto
    {
        public int MaxPlayers { get; set; } = 2;
    }

    public class JoinRoomDto
    {
        public string RoomId { get; set; } = null!;
    }

    public class RoomDto
    {
        public string RoomId { get; set; } = null!;
        public string HostUserId { get; set; } = null!;
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public string Status { get; set; } = null!;
        public List<RoomPlayerDto> Players { get; set; } = new();
    }

    public class RoomPlayerDto
    {
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public bool IsReady { get; set; }
        public bool IsHost { get; set; }
    }

    public class GameMoveDto
    {
        public string RoomId { get; set; } = null!;
        public string MoveType { get; set; } = null!; // "move" or "wall"
        public int ToRow { get; set; }
        public int ToCol { get; set; }
        public string? WallOrientation { get; set; } // "horizontal" or "vertical"
    }
}
