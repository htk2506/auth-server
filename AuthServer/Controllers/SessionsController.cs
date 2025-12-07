using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Dto.Sessions.Login;
using AuthServer.Helpers;
using AuthServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<AppUser> _passwordHasher;
        private readonly JwtTokenService _tokenService;

        public SessionsController(
            IConfiguration configuration,
            AppDbContext dbContext,
            PasswordHasher<AppUser> passwordHasher,
            JwtTokenService tokenService
        )
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
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

                // Generate a session token 
                string sessionToken = _tokenService.GenerateJwtToken(user.Id.ToString(), session.Id.ToString(), expiration);

                // Return token 
                return Ok(new LoginUserResponseBody { SessionToken = sessionToken });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }
    }
}