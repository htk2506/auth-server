using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        [Route("unprotected")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult UnprotectedGet()
        {
            return Ok("Hello World! This route doesn't require authentication");
        }
    }
}
