using Asp.Versioning;
using AuthServer.Api.V1.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthServer.Api.V1.Controllers
{
    [ApiController]
    [ApiVersion(1)]
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
                Message = "Hello World! This route doesn't require authentication."
            };

            _logger.LogInformation("Response body: {@ResponseBody}.", messageResponseBody);
            return Ok(messageResponseBody);
        }

        [Authorize]
        [HttpGet("protected")]
        [ProducesResponseType(typeof(MessageResponseBody), StatusCodes.Status200OK)]
        public IActionResult Protected()
        {
            _logger.LogInformation("Request received at {@RequestPath}.", Request.Path.Value);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            MessageResponseBody messageResponseBody = new MessageResponseBody
            {
                Message = "Hello! You are authorized."
            };

            _logger.LogInformation("Response body: {@ResponseBody}.", messageResponseBody);
            return Ok(messageResponseBody);
        }
    }
}
