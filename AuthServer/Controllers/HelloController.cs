using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        [Route("unprotected")]
        [HttpGet]
        public IActionResult UnprotectedGet()
        {
            return Ok("Hello World! This route doesn't require authentication");
        }
    }
}
