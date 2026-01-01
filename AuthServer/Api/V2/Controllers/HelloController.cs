using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.V2.Controllers
{
    [ApiController]
    [ApiVersion(2)]
    [Route("v{version:apiVersion}/[controller]")]
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
