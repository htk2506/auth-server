using AuthServer.Database.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

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
            string allowed = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
            int tokenLength = 9;
            char[] randomChars = new char[tokenLength];

            for (int i = 0; i < tokenLength; i++)
            {
                randomChars[i] = allowed[RandomNumberGenerator.GetInt32(0, allowed.Length)];
            }

            return new string(randomChars);
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
