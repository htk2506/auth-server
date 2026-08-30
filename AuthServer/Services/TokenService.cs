using AuthServer.Database.Models;
using AuthServer.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace AuthServer.Services
{
    public class TokenService : ITokenService
    {
        private readonly IPasswordHasher<AppUser> _passwordHasher;

        public TokenService(IPasswordHasher<AppUser> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string GenerateToken()
        {
            string allowed = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
            int tokenLength = 9;
            char[] randomChars = new char[tokenLength];

            for (int i = 0; i < tokenLength; i++)
            {
                randomChars[i] = allowed[RandomNumberGenerator.GetInt32(0, allowed.Length)];
            }

            return new string(randomChars).ToUpper();
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
