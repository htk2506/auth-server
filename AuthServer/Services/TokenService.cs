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

        public string GenerateToken()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        public string GenerateTokenHash(AppUser user, string token)
        {
            return _passwordHasher.HashPassword(user, token);
        }

        public bool VerifyHashedToken(AppUser user, string tokenHash, string token)
        {
            return _passwordHasher.VerifyHashedPassword(user, tokenHash, token) == PasswordVerificationResult.Success;
        }
    }
}
