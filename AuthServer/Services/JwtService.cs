using AuthServer.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthServer.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly RsaSecurityKey _privateKey;
        private readonly RsaSecurityKey _publicKey;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            RSACryptoServiceProvider rsaPrivate = new RSACryptoServiceProvider();
            rsaPrivate.FromXmlString(_configuration["Jwt:PrivateKey"] ?? throw new InvalidOperationException("Missing Jwt:PrivateKey."));
            _privateKey = new RsaSecurityKey(rsaPrivate);

            RSACryptoServiceProvider rsaPublic = new RSACryptoServiceProvider();
            rsaPublic.FromXmlString(configuration["Jwt:PublicKey"] ?? throw new InvalidOperationException("Missing Jwt:PublicKey."));
            _publicKey = new RsaSecurityKey(rsaPublic);
        }

        public string GenerateJwt(string userId, string jti, DateTimeOffset expiration)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            var credentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration.UtcDateTime,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateJwt(string token, out JwtSecurityToken? jwt)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = [_publicKey],
                ValidateLifetime = true
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                jwt = (JwtSecurityToken)validatedToken;

                return true;
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid JWT.");
                jwt = null;
                return false;
            }
        }
    }
}
