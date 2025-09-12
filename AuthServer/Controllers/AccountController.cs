using Microsoft.AspNetCore.Mvc;
using AuthServer.Dto;
using AuthServer.Helpers;
using Microsoft.AspNetCore.Identity;
using AuthServer.Database.Models;
using AuthServer.Database;

namespace AuthServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(AppDbContext dbContext, PasswordHasher<User> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            // Create user to store
            User user = new User
            {
                Username = request.Username.ToLower(),
                Notes = request.Notes ?? ""
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // Validate the user model
            TryValidateModel(user);
            if (!ModelState.IsValid)
            {
                var modelErrors = Utils.GetModelErrors(ModelState);

                // Return failure 
                return BadRequest(modelErrors);
            }

            // Save user to database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Return success
            return Ok($"User {user.Username} registered");
        }
    }
}
