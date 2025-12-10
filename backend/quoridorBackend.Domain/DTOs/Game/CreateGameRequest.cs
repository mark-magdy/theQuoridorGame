using QuoridorBackend.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.DTOs.Game;

public class CreateGameRequest
{
    [Required]
    public GameSettings Settings { get; set; } = null!;
    public bool IsPrivate { get; set; }
}
