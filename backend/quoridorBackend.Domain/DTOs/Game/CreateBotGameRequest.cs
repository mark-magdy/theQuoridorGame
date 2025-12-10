using QuoridorBackend.Domain.Enums;
using QuoridorBackend.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.DTOs.Game;

public class CreateBotGameRequest
{
    [Required]
    public GameSettings Settings { get; set; } = null!;
    
    [Required]
    public BotDifficulty BotDifficulty { get; set; }
    
    public int BotPlayerIndex { get; set; } = 1; // Which player slot the bot takes (default: player 2)
}
