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
        private readonly ILogger<HelloController> _logger;

        public HelloController(
            ILogger<HelloController> logger
        )
        {
            _logger = logger;
        }

        [HttpGet("unprotected")]
        [ProducesResponseType(typeof(MessageResponseBody), StatusCodes.Status200OK)]
        public IActionResult Unprotected()
        {
            _logger.LogInformation("Request received at {@RequestPath}.", Request.Path.Value);

            MessageResponseBody messageResponseBody = new MessageResponseBody
            {
                Message = "Hello World! This is V2!"
            };

            _logger.LogInformation("Response body: {@ResponseBody}.", messageResponseBody);
            return Ok(messageResponseBody);
        }
    }
}
