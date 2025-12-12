namespace QuoridorBackend.Domain.Entities
{
    public class GameRoom
    {
        public string RoomId { get; set; } = null!;
        public string HostUserId { get; set; } = null!;
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public string Status { get; set; } = "Waiting"; // Waiting, InProgress, Completed
        public DateTime CreatedAt { get; set; }
        public string? GameId { get; set; } // Stores Guid as string
        
        // Navigation properties
        public List<GameRoomPlayer> Players { get; set; } = new();
    }

    public class GameRoomPlayer
    {
        public int Id { get; set; }
        public string RoomId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string ConnectionId { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
        public bool IsReady { get; set; }
        
        // Navigation properties
        public GameRoom Room { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
