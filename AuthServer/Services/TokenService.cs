using AuthServer.Database.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace AuthServer.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<AppUser> _passwordHasher;

        public TokenService(IConfiguration configuration, PasswordHasher<AppUser> passwordHasher)
        {
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Generates a token string.
        /// </summary>
        /// <returns>The token.</returns>
        public string GenerateToken()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        /// <summary>
        /// Hashes a token.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <returns>The hashed token.</returns>
        public string GenerateTokenHash(AppUser user, string token)
        {
            return _passwordHasher.HashPassword(user, token);
        }

        /// <summary>
        /// Verifies if a token hash matches a token.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="tokenHash"></param>
        /// <param name="token"></param>
        /// <returns>True if the token hash matches the token and false otherwise.</returns>
        public bool VerifyHashedToken(AppUser user, string tokenHash, string token)
        {
            return _passwordHasher.VerifyHashedPassword(user, tokenHash, token) == PasswordVerificationResult.Success;
        }
    }
}
