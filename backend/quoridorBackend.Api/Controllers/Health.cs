using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace quoridorBackend.Api.Controllers
{
    /// <summary>
    /// Controller for health check endpoint.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class Health : ControllerBase
    {
        /// <summary>
        /// Returns the health status of the API.
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new { status = "Healthy", timestamp = System.DateTime.UtcNow });
        }
    }
}
