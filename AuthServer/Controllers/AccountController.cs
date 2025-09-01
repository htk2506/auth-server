using Microsoft.AspNetCore.Mvc;
using AuthServer.Dto;
using AuthServer.Data;
using AuthServer.Data.Models;
using AuthServer.Helpers;

namespace AuthServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AccountController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
        {
            User user = new User
            {
                Username = request.Username.ToLower(),
                PasswordHash = request.Password,
                Notes = request.Notes ?? ""
            };

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
