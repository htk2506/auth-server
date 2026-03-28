using Asp.Versioning;
using AuthServer.Api.V2.Dto;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.V2.Controllers
{
    [ApiController]
    [ApiVersion(2)]
    [Route("v{version:apiVersion}/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet("unprotected")]
        [ProducesResponseType(typeof(MessageResponseBody), StatusCodes.Status200OK)]
        public IActionResult Unprotected()
        {
            return Ok(new MessageResponseBody
            {
                Message = "Hello World! This is V2!"
            });
        }
    }
}
