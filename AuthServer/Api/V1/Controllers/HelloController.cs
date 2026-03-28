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
        [HttpGet("unprotected")]
        [ProducesResponseType(typeof(MessageResponseBody), StatusCodes.Status200OK)]
        public IActionResult Unprotected()
        {
            return Ok(new MessageResponseBody
            {
                Message = "Hello World! This route doesn't require authentication."
            });
        }

        [Authorize]
        [HttpGet("protected")]
        [ProducesResponseType(typeof(MessageResponseBody), StatusCodes.Status200OK)]
        public IActionResult Protected()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            return Ok(new MessageResponseBody
            {
                Message = "Hello! You are authorized."
            });
        }
    }
}
