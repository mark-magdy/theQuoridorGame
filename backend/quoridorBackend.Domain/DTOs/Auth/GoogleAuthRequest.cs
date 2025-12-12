using System.ComponentModel.DataAnnotations;

namespace QuoridorBackend.Domain.DTOs.Auth;

public class GoogleAuthRequest
{
    [Required]
    public string GoogleToken { get; set; } = string.Empty;
}
