using AuthServer.Database.Models;

namespace AuthServer.Services.Interfaces
{
    /// <summary>
    /// Generates tokens that aren't JWTs.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a token string.
        /// </summary>
        /// <returns>The token.</returns>
        string GenerateToken();

        /// <summary>
        /// Hashes a token.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns>The hashed token.</returns>
        string GenerateTokenHash(AppUser user, string token);

        /// <summary>
        /// Verifies if a token hash matches a token.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="tokenHash"></param>
        /// <param name="token"></param>
        /// <returns>True if the token hash matches the token and false otherwise.</returns>
        bool VerifyHashedToken(AppUser user, string tokenHash, string token);
    }
}
