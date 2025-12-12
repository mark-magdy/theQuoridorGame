using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.DAL.Repositories.Interfaces;
using QuoridorBackend.Domain.DTOs.Auth;
using QuoridorBackend.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace QuoridorBackend.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _configuration = configuration;
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
        
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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

    public async Task<AuthResponse> GoogleAuthAsync(GoogleAuthRequest request)
    {
        try
        {
            // Verify Google token
            var googleClientId = _configuration["Authentication:Google:ClientId"];
            if (string.IsNullOrEmpty(googleClientId))
            {
                throw new InvalidOperationException("Google Client ID not configured");
            }

            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.GoogleToken, validationSettings);

            // Check if user exists by Google ID or email
            var user = await _unitOfWork.Users.GetByEmailAsync(payload.Email);

            if (user == null)
            {
                // Create new user for Google sign-in
                user = new User
                {
                    Username = payload.Name ?? payload.Email.Split('@')[0],
                    Email = payload.Email,
                    GoogleId = payload.Subject,
                    PasswordHash = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Ensure username is unique
                var existingUserByUsername = await _unitOfWork.Users.GetByUsernameAsync(user.Username);
                if (existingUserByUsername != null)
                {
                    user.Username = $"{user.Username}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

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

                _unitOfWork.Context.UserStats.Add(stats);
                await _unitOfWork.CompleteAsync();
            }
            else
            {
                // Update Google ID if not set
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = payload.Subject;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.CompleteAsync();
                }
            }

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
        catch (InvalidJwtException)
        {
            throw new UnauthorizedAccessException("Invalid Google token");
        }
    }
}
