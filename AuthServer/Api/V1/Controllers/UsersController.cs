using Asp.Versioning;
using AuthServer.Api.V1.Dto.Users.Create;
using AuthServer.Api.V1.Dto.Users.Get;
using AuthServer.Api.V1.Dto.Users.Update;
using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Helpers;
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
                // Check if username already taken (even by a deleted user) 
                AppUser? existingUser = _dbContext.AppUsers.IgnoreQueryFilters().FirstOrDefault(x => x.Username.Equals(requestBody.Username.ToLower()));
                if (existingUser != null) { return BadRequest("Username not available."); }

                // Create user to store
                AppUser user = new AppUser
                {
                    Username = requestBody.Username.ToLower(),
                    Note = requestBody.Note
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

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(GetUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                // Get the user
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                AppUser? user = _dbContext.AppUsers.Find(Guid.Parse(userId));
                if (user == null) { return BadRequest("User not found."); }

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

        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(typeof(UpdateUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequestBody requestBody)
        {
            try
            {
                // Get the user
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                AppUser? user = _dbContext.AppUsers.Find(Guid.Parse(userId));
                if (user == null) { return BadRequest("User not found."); }

                // Check if new username already taken (even by a deleted user) 
                if (!user.Username.ToLower().Equals(requestBody.Username.ToLower()))
                {
                    AppUser? existingUser = _dbContext.AppUsers.IgnoreQueryFilters().FirstOrDefault(x => x.Username.Equals(requestBody.Username.ToLower()));
                    if (existingUser != null) { return BadRequest("Username not available."); }
                }

                // Modify the user
                user.Username = requestBody.Username.ToLower();
                user.Note = requestBody.Note;

                // Validate the user model
                TryValidateModel(user);
                if (!ModelState.IsValid) { return BadRequest(Utils.GetModelErrors(ModelState)); }

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                // Return success
                return Ok(new UpdateUserResponseBody
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

        [Authorize]
        [HttpDelete("me")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                // Get the user
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                AppUser? user = _dbContext.AppUsers.Find(Guid.Parse(userId));
                if (user == null) { return BadRequest("User not found."); }

                // Generate a new username for the deleted user
                string newUsername;
                AppUser? conflictingUser;
                do
                {
                    newUsername = $"deleted_{DateTimeOffset.UtcNow.Ticks.ToString("x").ToLower()}";
                    conflictingUser = _dbContext.AppUsers.IgnoreQueryFilters().FirstOrDefault(x => x.Username.Equals(newUsername));
                } while (conflictingUser != null);
                user.Username = newUsername;

                // Delete the user
                _dbContext.AppUsers.Remove(user);
                await _dbContext.SaveChangesAsync();

                // Return success
                return Ok(true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }
    }
}