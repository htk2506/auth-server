using AuthServer.Database.Models;

namespace AuthServer.Services.Interfaces
{
    /// <summary>
    /// Sends email messages.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a user an email with a token for resetting their password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <exception cref="InvalidOperationException"></exception>
        Task SendPasswordResetTokenEmail(AppUser user, string token);
    }
}
