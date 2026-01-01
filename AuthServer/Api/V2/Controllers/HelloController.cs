using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.V2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet("unprotected")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Unprotected()
        {
            return Ok("Hello World! This is V2!");
        }
    }
}
