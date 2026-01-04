using Asp.Versioning;
using AuthServer.Api.V1.Dto.Sessions.Login;
using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Helpers;
using AuthServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthServer.Api.V1.Controllers
{
    [ApiController]
    [ApiVersion(1)]
    [Route("v{version:apiVersion}/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<AppUser> _passwordHasher;
        private readonly JwtService _jwtService;

        public SessionsController(
            IConfiguration configuration,
            AppDbContext dbContext,
            PasswordHasher<AppUser> passwordHasher,
            JwtService jwtService
        )
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginUserRequestBody requestBody)
        {
            try
            {
                // Attempt to get the user
                string username = requestBody.Username.ToLower();
                AppUser? user = _dbContext.AppUsers.FirstOrDefault(x => x.Username.Equals(username));
                if (user == null) { return Unauthorized("User not found."); }

                // Check the password hash
                PasswordVerificationResult passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestBody.Password);
                if (passwordVerificationResult != PasswordVerificationResult.Success) { return Unauthorized("Invalid credentials."); }

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
                if (!ModelState.IsValid) { return BadRequest(Utils.GetModelErrors(ModelState)); }

                // Save session to database
                _dbContext.UserSessions.Add(session);
                await _dbContext.SaveChangesAsync();

                // Generate a session token with user ID as subject and session ID as JTI
                string sessionToken = _jwtService.GenerateJwt(user.Id.ToString(), session.Id.ToString(), expiration);

                // Return token 
                return Ok(new LoginUserResponseBody { SessionToken = sessionToken });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get the session
                string sessionId = User.FindFirstValue(ClaimTypes.Authentication) ?? "";
                UserSession? session = _dbContext.UserSessions.Find(Guid.Parse(sessionId));
                if (session == null) { return BadRequest("Session not found."); }

                // Remove session from database
                _dbContext.UserSessions.Remove(session);
                await _dbContext.SaveChangesAsync();

                // Return token 
                return Ok("Logout successful.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }
    }
}