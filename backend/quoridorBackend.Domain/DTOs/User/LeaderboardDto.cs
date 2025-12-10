namespace QuoridorBackend.Domain.DTOs.User;

public class LeaderboardDto
{
    public List<LeaderboardEntryDto> Entries { get; set; } = new();
    public int TotalCount { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}
