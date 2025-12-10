using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.DAL.Repositories.Interfaces;
using QuoridorBackend.Domain.DTOs.Auth;
using QuoridorBackend.Domain.Entities;

namespace QuoridorBackend.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUserByEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUserByEmail != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var existingUserByUsername = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        if (existingUserByUsername != null)
        {
            throw new InvalidOperationException("Username is already taken");
        }

        // Create user with hashed password
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);

        // Create initial stats
        var stats = new UserStats
        {
            UserId = user.Id,
            GamesPlayed = 0,
            GamesWon = 0,
            TotalMoves = 0,
            WallsPlaced = 0,
            UpdatedAt = DateTime.UtcNow
        };

        // Add stats to context
        _unitOfWork.Context.UserStats.Add(stats);
        
        await _unitOfWork.CompleteAsync();

        // Generate token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            },
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            },
            Token = token
        };
    }
}
