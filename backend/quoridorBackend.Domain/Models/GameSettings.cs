namespace QuoridorBackend.Domain.Models;

public class GameSettings
{
    public int PlayerCount { get; set; } // 2, 3, or 4
    public int BoardSize { get; set; } // 7, 9, or 11
    public string Theme { get; set; } = "light";
    public bool ShowValidPaths { get; set; }
}
