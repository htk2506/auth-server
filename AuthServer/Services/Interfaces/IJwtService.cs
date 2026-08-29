using System.IdentityModel.Tokens.Jwt;

namespace AuthServer.Services.Interfaces
{
    /// <summary>
    /// Generates and validates JSON Web Tokens (JWTs).
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="jti"></param>
        /// <param name="expiration"></param>
        /// <returns>The JWT.</returns>
        string GenerateJwt(string userId, string jti, DateTimeOffset expiration);

        /// <summary>
        /// Validates a JWT.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="jwt"></param>
        /// <returns>True if JWT is valid and false otherwise.</returns>
        bool ValidateJwt(string token, out JwtSecurityToken? jwt);
    }
}
