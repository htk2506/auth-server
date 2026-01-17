using Asp.Versioning;
using AuthServer.Api.V1.Dto.Users.Create;
using AuthServer.Api.V1.Dto.Users.Get;
using AuthServer.Api.V1.Dto.Users.PasswordReset;
using AuthServer.Api.V1.Dto.Users.Update;
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
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<AppUser> _passwordHasher;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;

        public UsersController(
            IConfiguration configuration,
            AppDbContext dbContext,
            PasswordHasher<AppUser> passwordHasher,
            EmailService emailService,
            TokenService tokenService
        )
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        #region v1/users
        [HttpPost]
        [ProducesResponseType(typeof(CreateUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestBody requestBody)
        {
            try
            {
                // Check if username already taken 
                if (IsUsernameTaken(requestBody.Username)) { return BadRequest("Username not available."); }

                // Check if email already taken 
                if (IsEmailTaken(requestBody.Email)) { return BadRequest("Email not available."); }

                // Create user to store
                AppUser user = new AppUser
                {
                    Username = requestBody.Username.ToLower(),
                    Email = requestBody.Email?.ToLower(),
                    Note = requestBody.Note,
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
                    Email = user.Email,
                    Note = user.Note,
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }
        #endregion

        #region v1/users/me
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
                    Email = user.Email,
                    Username = user.Username,
                    Note = user.Note,
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

                // Check if new username already taken
                if (!user.Username.ToLower().Equals(requestBody.Username.ToLower()))
                {
                    if (IsUsernameTaken(requestBody.Username)) { return BadRequest("Username not available."); }
                }

                // Check if new email already taken
                if (!(requestBody.Email == null || requestBody.Email.ToLower() == user.Email?.ToLower()))
                {
                    if (IsEmailTaken(requestBody.Email)) { return BadRequest("Email not available."); }
                }

                // Modify the user
                user.Username = requestBody.Username.ToLower();
                user.Email = requestBody.Email?.ToLower();
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
                    Email = user.Email,
                    Note = user.Note,
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
                do
                {
                    newUsername = $"deleted_{DateTimeOffset.UtcNow.Ticks.ToString("x").ToLower()}";
                } while (IsUsernameTaken(newUsername));
                user.Username = newUsername;

                // Remove the email
                user.Email = null;

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
        #endregion

        #region v1/users/me/password
        [Authorize]
        [HttpPut("me/password")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserPasswordRequestBody requestBody)
        {
            try
            {
                // Get the user
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                AppUser? user = _dbContext.AppUsers.Find(Guid.Parse(userId));
                if (user == null) { return BadRequest("User not found."); }

                // Verify old password is correct
                PasswordVerificationResult passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, requestBody.CurrentPassword);
                if (passwordVerificationResult != PasswordVerificationResult.Success) { return Unauthorized("Invalid credentials."); }

                // Modify the user
                user.PasswordHash = _passwordHasher.HashPassword(user, requestBody.NewPassword);

                // Validate the user model
                TryValidateModel(user);
                if (!ModelState.IsValid) { return BadRequest(Utils.GetModelErrors(ModelState)); }

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                // Log user out of their sessions
                EndSessionsOfUser(user);

                // Return success
                return Ok(true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }
        #endregion

        [Authorize]
        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail([FromQuery] string recipientName, [FromQuery] string recipientEmail)
        {
            try
            {
                Console.Write("Sending email");
                await _emailService.SendTestEmail(recipientName, recipientEmail);

                return Ok("Testing");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Problem("Error occurred.");
            }
        }

        [HttpPost("password-reset-request")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartPasswordReset([FromQuery] StartPasswordResetRequestBody requestBody)
        {
            // Get the user the email belongs to
            AppUser? existingUser = _dbContext.AppUsers.FirstOrDefault(x => x.Email == requestBody.Email.ToLower());
            if (existingUser == null) { return BadRequest("User not found."); }

            // Generate the password reset token and its hash
            string token = _tokenService.GenerateToken();
            string tokenHash = _tokenService.GenerateTokenHash(existingUser, token);

            // Save to the database to use later
            PasswordResetToken passwordResetToken = new PasswordResetToken
            {
                AppUser = existingUser,
                TokenHash = tokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:SessionDays"))
            };
            TryValidateModel(passwordResetToken);
            if (!ModelState.IsValid) { return BadRequest(Utils.GetModelErrors(ModelState)); }
            await _dbContext.SaveChangesAsync();

            // Send the email
            await _emailService.SendPasswordResetTokenEmail(existingUser, token);
            return Ok("Email sent.");
        }

        /// <summary>
        /// Checks if the username is taken by an existing or soft-deleted user.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>True if username is taken and false otherwise.</returns>
        private bool IsUsernameTaken(string username)
        {
            // Check if a user exists with the username
            AppUser? existingUser = _dbContext.AppUsers.IgnoreQueryFilters().FirstOrDefault(x => x.Username.Equals(username.ToLower()));
            return existingUser != null;
        }

        /// <summary>
        /// Checks if the email is taken by an existing or soft-deleted user.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>True if email is taken and false otherwise.</returns>
        private bool IsEmailTaken(string? email)
        {
            if (email == null) { return false; }

            // Check if a user exists with the email
            AppUser? existingUser = _dbContext.AppUsers.IgnoreQueryFilters().FirstOrDefault(x => x.Email == email.ToLower());
            return existingUser != null;
        }

        /// <summary>
        /// Ends all of a user's sessions.  
        /// </summary>
        /// <param name="user"></param>
        private void EndSessionsOfUser(AppUser user)
        {
            var sessions = _dbContext.UserSessions.Where(userSession => userSession.AppUser == user);
            _dbContext.UserSessions.RemoveRange(sessions);
            _dbContext.SaveChanges();
        }
    }
}