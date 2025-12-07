using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Dto.Users.Create;
using AuthServer.Dto.Users.Get;
using AuthServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                AppUser? existingUser = _dbContext.AppUsers.FirstOrDefault(x => x.Username.Equals(requestBody.Username.ToLower()));
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
                if (!ModelState.IsValid) { return BadRequest(Utils.GetModelErrors(ModelState)); }

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

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(GetUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                AppUser? user = _dbContext.AppUsers.Find(Guid.Parse(userId));
                if (user == null) { return BadRequest("User not found"); }

                // Return success
                return Ok(new GetUserResponseBody
                {
                    Id = user.Id,
                    Username = user.Username,
                    Note = user.Note
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