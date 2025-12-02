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
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly TokenService _tokenService;

        public SessionsController(
            AppDbContext dbContext,
            PasswordHasher<User> passwordHasher,
            TokenService tokenService
        )
        {
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
                User? user = _dbContext.Users.FirstOrDefault(user => user.Username.Equals(username));
                if (user == null) { return Unauthorized("User not found."); }

                // Check the password hash
                PasswordVerificationResult passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestBody.Password);
                if (passwordVerificationResult != PasswordVerificationResult.Success) { return Unauthorized("Invalid credentials."); }

                // Generate a session token 
                string sessionToken = _tokenService.GenerateJwtToken(user.Id, Guid.NewGuid());

                // TODO: Add a new session entry 


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