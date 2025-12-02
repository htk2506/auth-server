using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
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
            // TODO: Add more specific user
            return Ok("Hello user! This route is protected.");
        }
    }
}
