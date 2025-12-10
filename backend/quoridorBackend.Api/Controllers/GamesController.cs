using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoridorBackend.BLL.Services.Interfaces;
using QuoridorBackend.Domain.DTOs.Game;
using System.Security.Claims;

namespace QuoridorBackend.Api.Controllers
{
    /// <summary>
    /// Controller for game-related operations
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
     [Authorize]
    public class GamesController : ControllerBase
    {
        private readonly ILogger<GamesController> _logger;
        private readonly IGameService _gameService;

        /// <summary>
        /// Constructor for GamesController
        /// </summary>
        public GamesController(ILogger<GamesController> logger, IGameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }


        /// <summary>
        /// Create a new game against a bot
        /// </summary>
        [HttpPost("bot")]
        public async Task<IActionResult> CreateBotGame([FromBody] CreateBotGameRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _gameService.CreateBotGameAsync(userId, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot game");
                return StatusCode(500, new { message = "Error creating game" });
            }
        }


        /// <summary>
        /// Make a move in a game
        /// </summary>
        [HttpPost("{gameId}/moves")]
        public async Task<IActionResult> MakeMove(Guid gameId, [FromBody] MakeMoveRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _gameService.MakeMoveAsync(gameId, userId, request);
                
                if (!response.IsValid)
                    return BadRequest(new { message = response.Error });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making move");
                return StatusCode(500, new { message = "Error making move" });
            }
        }

        /// <summary>
        /// Delete a game
        /// </summary>
        [HttpDelete("{gameId}")]
        public async Task<IActionResult> DeleteGame(Guid gameId)
        {
            try
            {
                var userId = GetUserId();
                var success = await _gameService.DeleteGameAsync(gameId, userId);
                
                if (!success)
                    return NotFound(new { message = "Game not found or unauthorized" });

                return Ok(new { message = "Game deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting game");
                return StatusCode(500, new { message = "Error deleting game" });
            }
        }


        //// <summary>
        //// Get available moves for current player in a game
        //// </summary>
        // [HttpGet("{gameId}/available-moves")]
        // public async Task<IActionResult> GetAvailableMoves(Guid gameId)
        // {
        //     try
        //     {
        //         var userId = GetUserId();
        //         var moves = await _gameService.GetAvailableMovesAsync(gameId, userId);
        //         return Ok(moves);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting available moves");
        //         return StatusCode(500, new { message = "Error retrieving available moves" });
        //     }
        // }

        /// <summary>
        /// Get a specific game by ID
        /// </summary>
        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetGame(Guid gameId)
        {
            try
            {
                var userId = GetUserId();
                var game = await _gameService.GetGameAsync(gameId, userId);
                
                if (game == null)
                    return NotFound(new { message = "Game not found" });

                return Ok(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game");
                return StatusCode(500, new { message = "Error retrieving game" });
            }
        }

        /// <summary>
        /// Get all active games for the current user
        /// </summary>
        [HttpGet("my-games")]
        public async Task<IActionResult> GetMyGames()
        {
            try
            {
                var userId = GetUserId();
                var games = await _gameService.GetUserGamesAsync(userId);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user games");
                return StatusCode(500, new { message = "Error retrieving games" });
            }
        }

        /// <summary>
        /// Get all finished games for the current user
        /// </summary>
        [HttpGet("my-games/finished")]
        public async Task<IActionResult> GetMyFinishedGames()
        {
            try
            {
                var userId = GetUserId();
                var games = await _gameService.GetUserFinishedGamesAsync(userId);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting finished games");
                return StatusCode(500, new { message = "Error retrieving finished games" });
            }
        }
        /// <summary>
        /// Create a new multiplayer game (future implementation)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMultiPlayerGame()
        {
            return Ok(new { message = "Multiplayer game creation coming soon!" });
        }
    }
}