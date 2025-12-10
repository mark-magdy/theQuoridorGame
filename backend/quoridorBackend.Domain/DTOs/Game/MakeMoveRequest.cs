using QuoridorBackend.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.DTOs.Game;

public class MakeMoveRequest
{
    [Required]
    public Move Move { get; set; } = null!;
}
