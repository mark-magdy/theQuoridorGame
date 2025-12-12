using QuoridorBackend.Domain.DTOs.Auth;

namespace QuoridorBackend.BLL.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request);
}
