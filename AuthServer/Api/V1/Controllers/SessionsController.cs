using Asp.Versioning;
using AuthServer.Api.V1.Dto;
using AuthServer.Api.V1.Dto.Sessions.Login;
using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Helpers;
using AuthServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthServer.Api.V1.Controllers
{
    [ApiController]
    [ApiVersion(1)]
    [Route("v{version:apiVersion}/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly JwtService _jwtService;

        public SessionsController(
            ILogger<SessionsController> logger,
            IConfiguration configuration,
            AppDbContext dbContext,
            IPasswordHasher<AppUser> passwordHasher,
            JwtService jwtService
        )
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginUserRequestBody requestBody)
        {
            _logger.LogInformation("Request received at {@RequestPath}. RequestBody: {@RequestBody}.", Request.Path.Value, requestBody);

            // Attempt to get the user
            string username = requestBody.Username.ToLower();
            AppUser? user = await _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Username.Equals(username));
            if (user == null)
            {
                _logger.LogInformation("Problem StatusCode: {@StatusCode}. Detail: {@Detail}.", StatusCodes.Status401Unauthorized, "Invalid credentials.");
                return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid credentials.");
            }

            // Check the password hash
            PasswordVerificationResult passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestBody.Password);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                _logger.LogInformation("Problem StatusCode: {@StatusCode}. Detail: {@Detail}.", StatusCodes.Status401Unauthorized, "Invalid credentials.");
                return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid credentials.");
            }

            // Calculate the expiration timestamp 
            DateTimeOffset expiration = DateTimeOffset.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:SessionDays"));

            // Create the session
            UserSession session = new UserSession
            {
                AppUser = user,
                ExpiresAt = expiration
            };

            // Validate the session model
            TryValidateModel(session);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Problem StatusCode: {@StatusCode}. Detail: {@Detail}.", StatusCodes.Status400BadRequest, Utils.GetModelErrors(ModelState));
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: Utils.GetModelErrors(ModelState));
            }

            // Save session to database
            _logger.LogDebug("Saving user session: {@UserSession}.", session);
            await _dbContext.UserSessions.AddAsync(session);
            await _dbContext.SaveChangesAsync();

            // Generate a session token with user ID as subject and session ID as JTI
            string sessionToken = _jwtService.GenerateJwt(user.Id.ToString(), session.Id.ToString(), expiration);

            // Return token 
            LoginUserResponseBody loginResponseBody = new LoginUserResponseBody
            {
                SessionToken = sessionToken
            };
            _logger.LogInformation("Response body: {@ResponseBody}.", loginResponseBody);
            return Ok(loginResponseBody);
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(MessageResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Request received at {@RequestPath}.", Request.Path.Value);

            // Get the session
            string sessionId = User.FindFirstValue(ClaimTypes.Authentication) ?? "";
            UserSession? session = await _dbContext.UserSessions.FindAsync(Guid.Parse(sessionId));
            if (session == null)
            {
                _logger.LogInformation("Problem StatusCode: {@StatusCode}. Detail: {@Detail}.", StatusCodes.Status400BadRequest, "Session not found.");
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Session not found.");
            }

            // Remove session from database
            _logger.LogDebug("Deleting user session: {@UserSession}.", session);
            _dbContext.UserSessions.Remove(session);
            await _dbContext.SaveChangesAsync();

            // Return token 
            MessageResponseBody messageResponseBody = new MessageResponseBody
            {
                Message = "Logout successful."
            };
            _logger.LogInformation("Response body: {@ResponseBody}.", messageResponseBody);
            return Ok(messageResponseBody);
        }
    }
}