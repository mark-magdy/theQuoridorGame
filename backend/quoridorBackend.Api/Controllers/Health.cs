using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace quoridorBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Health : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new { status = "Healthy", timestamp = System.DateTime.UtcNow });
        }
    }
}
