using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthServer.Api.V1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet("unprotected")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Unprotected()
        {
            return Ok("Hello World! This route doesn't require authentication.");
        }

        [Authorize]
        [HttpGet("protected")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Protected()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            return Ok($"Hello! You are authorized.");
        }
    }
}
