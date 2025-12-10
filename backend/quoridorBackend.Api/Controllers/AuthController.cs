using Microsoft.AspNetCore.Mvc;
using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.Domain.DTOs.Auth;
using QuoridorBackend.Domain.DTOs.Common;

namespace QuoridorBackend.Api.Controllers;

/// <summary>
/// Authentication endpoints for user registration and login
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController
    /// </summary>
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details including username, email, and password</param>
    /// <returns>User information and JWT token</returns>
    /// <response code="200">Successfully registered and logged in</response>
    /// <response code="400">Invalid request or user already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            _logger.LogInformation("User {Username} registered successfully", request.Username);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return BadRequest(new ErrorResponse { Message = "Registration failed. Please try again." });
        }
    }

    /// <summary>
    /// Login with existing credentials
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <returns>User information and JWT token</returns>
    /// <response code="200">Successfully logged in</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(500, new ErrorResponse { Message = "Login failed. Please try again." });
        }
    }
}
