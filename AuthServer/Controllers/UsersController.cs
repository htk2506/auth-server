using Microsoft.AspNetCore.Mvc;
using AuthServer.Helpers;
using Microsoft.AspNetCore.Identity;
using AuthServer.Database.Models;
using AuthServer.Database;
using AuthServer.Dto.Users.Create;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public UsersController(AppDbContext dbContext, PasswordHasher<AppUser> passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestBody requestBody)
        {
            try
            {
                // Check if username already taken
                AppUser? existingUser = _dbContext.AppUsers.FirstOrDefault(user => user.Username.Equals(requestBody.Username.ToLower()));
                if (existingUser != null) { return BadRequest("Username taken."); }

                // Create user to store
                AppUser user = new AppUser
                {
                    Username = requestBody.Username.ToLower(),
                    Note = requestBody.Note ?? "",
                };
                user.PasswordHash = _passwordHasher.HashPassword(user, requestBody.Password);

                // Validate the user model
                TryValidateModel(user);
                if (!ModelState.IsValid)
                {
                    var modelErrors = Utils.GetModelErrors(ModelState);

                    // Return failure 
                    return BadRequest(modelErrors);
                }

                // Save user to database
                _dbContext.AppUsers.Add(user);
                await _dbContext.SaveChangesAsync();

                // Return success
                return Ok(new CreateUserResponseBody
                {
                    Id = user.Id,
                    Username = user.Username
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }
    }
}