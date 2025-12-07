using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Dto.Sessions.Login;
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
                AppUser? user = _dbContext.AppUsers.FirstOrDefault(user => user.Username.Equals(username));
                if (user == null) { return Unauthorized("User not found."); }

                // Check the password hash
                PasswordVerificationResult passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestBody.Password);
                if (passwordVerificationResult != PasswordVerificationResult.Success) { return Unauthorized("Invalid credentials."); }

                DateTimeOffset expiration = DateTimeOffset.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:SessionDays"));
                
                // TODO: Add a new session entry 
                Guid sessionId = Guid.NewGuid(); // TODO: Get this off the database

                // Generate a session token 
                string sessionToken = _tokenService.GenerateJwtToken(user.Id.ToString(), sessionId.ToString(), expiration);

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