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
            // Check if username already taken 
            if (await IsUsernameTaken(requestBody.Username)) { return BadRequest("Username not available."); }

            // Check if email already taken 
            if (await IsEmailTaken(requestBody.Email)) { return BadRequest("Email not available."); }

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
            await _dbContext.AppUsers.AddAsync(user);
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
        #endregion

        #region v1/users/me
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(GetUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser()
        {
            // Get the user
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            AppUser? user = await _dbContext.AppUsers.FindAsync(Guid.Parse(userId));
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

        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(typeof(UpdateUserResponseBody), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequestBody requestBody)
        {
            // Get the user
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            AppUser? user = await _dbContext.AppUsers.FindAsync(Guid.Parse(userId));
            if (user == null) { return BadRequest("User not found."); }

            // Check if new username already taken
            if (!user.Username.ToLower().Equals(requestBody.Username.ToLower()))
            {
                if (await IsUsernameTaken(requestBody.Username)) { return BadRequest("Username not available."); }
            }

            // Check if new email already taken
            if (!(requestBody.Email == null || requestBody.Email.ToLower() == user.Email?.ToLower()))
            {
                if (await IsEmailTaken(requestBody.Email)) { return BadRequest("Email not available."); }
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

        [Authorize]
        [HttpDelete("me")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteUser()
        {
            // Get the user
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            AppUser? user = await _dbContext.AppUsers.FindAsync(Guid.Parse(userId));
            if (user == null) { return BadRequest("User not found."); }

            // Generate a new username for the deleted user
            string newUsername;
            do
            {
                newUsername = $"deleted_{DateTimeOffset.UtcNow.Ticks.ToString("x").ToLower()}";
            } while (await IsUsernameTaken(newUsername));
            user.Username = newUsername;

            // Remove the email
            user.Email = null;

            // Delete the user
            _dbContext.AppUsers.Remove(user);
            await _dbContext.SaveChangesAsync();

            // Return success
            return Ok(true);
        }
        #endregion

        #region v1/users/me/password
        [Authorize]
        [HttpPut("me/password")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserPasswordRequestBody requestBody)
        {
            // Get the user
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            AppUser? user = await _dbContext.AppUsers.FindAsync(Guid.Parse(userId));
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
            await EndSessionsOfUser(user);

            // Return success
            return Ok(true);
        }
        #endregion

        #region v1/users/password-reset-request
        [HttpPost("password-reset-request")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartPasswordReset([FromBody] StartPasswordResetRequestBody requestBody)
        {
            // Get the user the email belongs to
            AppUser? existingUser = await _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Email == requestBody.Email.ToLower());
            if (existingUser == null) { return BadRequest("User not found."); }

            // Generate the password reset token and its hash
            string token = _tokenService.GenerateToken();
            string tokenHash = _tokenService.GenerateTokenHash(existingUser, token);

            // Save to the database to use later
            PasswordResetToken passwordResetToken = new PasswordResetToken
            {
                AppUser = existingUser,
                TokenHash = tokenHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_configuration.GetValue<int>("PasswordResetToken:Minutes"))
            };
            TryValidateModel(passwordResetToken);
            if (!ModelState.IsValid) { return BadRequest(Utils.GetModelErrors(ModelState)); }
            await _dbContext.PasswordResetTokens.AddAsync(passwordResetToken);
            await _dbContext.SaveChangesAsync();

            // Send the email
            await _emailService.SendPasswordResetTokenEmail(existingUser, token);

            // Return success
            return Ok("Email sent.");
        }
        #endregion

        #region v1/users/password-reset
        [HttpPost("password-reset")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequestBody requestBody)
        {
            // Get the user for the request
            AppUser? existingUser = await _dbContext.AppUsers.FindAsync(requestBody.Id);
            if (existingUser == null) { return BadRequest("User not found."); }

            // Get list of unexpired password reset tokens that belong to the user
            List<PasswordResetToken> passwordResetTokens = await _dbContext.PasswordResetTokens
                .Where(x => DateTimeOffset.UtcNow < x.ExpiresAt && x.AppUser == existingUser)
                .ToListAsync();

            // Check if passed in token matches any of the token hashes 
            PasswordResetToken? validPasswordResetToken = passwordResetTokens
                .Find(x => _tokenService.VerifyHashedToken(existingUser, x.TokenHash, requestBody.PasswordResetToken));
            if (validPasswordResetToken == null) { return Unauthorized("Invalid password reset token."); }

            // Set new password
            existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, requestBody.NewPassword);
            TryValidateModel(existingUser);
            if (!ModelState.IsValid) { return BadRequest(Utils.GetModelErrors(ModelState)); }

            // Delete other password reset tokens
            List<PasswordResetToken> otherPasswordResetTokens = await _dbContext.PasswordResetTokens
                .Where(x => x.AppUser == existingUser)
                .ToListAsync();
            _dbContext.PasswordResetTokens.RemoveRange(otherPasswordResetTokens);

            // Save changes
            await _dbContext.SaveChangesAsync();

            // Log user out of all existing sessions
            await EndSessionsOfUser(existingUser);

            // Return success
            return Ok(true);
        }
        #endregion

        /// <summary>
        /// Checks if the username is taken by an existing or soft-deleted user.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>True if username is taken and false otherwise.</returns>
        private async Task<bool> IsUsernameTaken(string username)
        {
            // Check if a user exists with the username
            AppUser? existingUser = await _dbContext.AppUsers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Username.Equals(username.ToLower()));
            return existingUser != null;
        }

        /// <summary>
        /// Checks if the email is taken by an existing or soft-deleted user.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>True if email is taken and false otherwise.</returns>
        private async Task<bool> IsEmailTaken(string? email)
        {
            if (email == null) { return false; }

            // Check if a user exists with the email
            AppUser? existingUser = await _dbContext.AppUsers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == email.ToLower());
            return existingUser != null;
        }

        /// <summary>
        /// Ends all of a user's sessions.  
        /// </summary>
        /// <param name="user"></param>
        private async Task EndSessionsOfUser(AppUser user)
        {
            List<UserSession> sessions = await _dbContext.UserSessions.Where(userSession => userSession.AppUser == user).ToListAsync();
            _dbContext.UserSessions.RemoveRange(sessions);
            await _dbContext.SaveChangesAsync();
        }
    }
}