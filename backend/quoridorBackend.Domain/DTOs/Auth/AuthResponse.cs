namespace QuoridorBackend.Domain.DTOs.Auth;

public class AuthResponse
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
