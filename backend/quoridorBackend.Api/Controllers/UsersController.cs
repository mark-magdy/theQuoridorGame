using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.Domain.DTOs.Common;
using QuoridorBackend.Domain.DTOs.User;
using System.Security.Claims;

namespace QuoridorBackend.Api.Controllers;

/// <summary>
/// User profile and statistics endpoints
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return Guid.Parse(userIdClaim);
    }

    /// <summary>
    /// Get the current authenticated user's profile
    /// </summary>
    /// <returns>User profile with statistics</returns>
    /// <response code="200">Successfully retrieved profile</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpGet("me")]
     [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Profile not found: {Message}", ex.Message);
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile");
            return StatusCode(500, new ErrorResponse { Message = "Failed to retrieve profile" });
        }
    }

    /// <summary>
    /// Get a specific user's profile by ID
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>User profile with statistics</returns>
    /// <response code="200">Successfully retrieved profile</response>
    /// <response code="404">User not found</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile([FromRoute] Guid userId)
    {
        try
        {
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for {UserId}", userId);
            return StatusCode(500, new ErrorResponse { Message = "Failed to retrieve profile" });
        }
    }

    /// <summary>
    /// Get a specific user's statistics by ID
    /// </summary>
    /// <param name="userId">The user's unique identifier</param>
    /// <returns>User game statistics</returns>
    /// <response code="200">Successfully retrieved statistics</response>
    /// <response code="404">User not found</response>
    [HttpGet("{userId:guid}/stats")]
    [ProducesResponseType(typeof(UserStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserStatsDto>> GetUserStats([FromRoute] Guid userId)
    {
        try
        {
            var stats = await _userService.GetStatsAsync(userId);
            return Ok(stats);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stats for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse { Message = "Failed to retrieve statistics" });
        }
    }

    /// <summary>
    /// Get the leaderboard of top players
    /// </summary>
    /// <param name="limit">Maximum number of entries to return (default: 50, max: 100)</param>
    /// <param name="offset">Number of entries to skip for pagination (default: 0)</param>
    /// <returns>Paginated leaderboard with player rankings</returns>
    /// <response code="200">Successfully retrieved leaderboard</response>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(LeaderboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LeaderboardDto>> GetLeaderboard(
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            // Validate parameters
            limit = Math.Min(Math.Max(1, limit), 100);
            offset = Math.Max(0, offset);

            var leaderboard = await _userService.GetLeaderboardAsync(limit, offset);
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard");
            return StatusCode(500, new ErrorResponse { Message = "Failed to retrieve leaderboard" });
        }
    }
}
