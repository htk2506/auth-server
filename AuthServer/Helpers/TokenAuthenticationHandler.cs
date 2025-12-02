using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AuthServer.Helpers
{
    public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtTokenService _tokenService;
        private readonly AppDbContext _dbContext;

        public TokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            JwtTokenService tokenService,
            AppDbContext dbContext
        ) : base(options, logger, encoder)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Get the auth header
            string? authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("Invalid authorization header.");
            }

            // Extract the token
            string token = authHeader.Substring("Bearer ".Length).Trim();

            // Validate the token 
            bool isTokenValid = _tokenService.ValidateJwtToken(token, out JwtSecurityToken? jwt);
            if (!isTokenValid || jwt == null)
            {
                return AuthenticateResult.Fail("Invalid token.");
            }

            // TODO: Make sure token is for valid session

            // Get the user
            User? user =_dbContext.Users.FirstOrDefault(x => x.Id.ToString().Equals(jwt.Subject));
            if (user == null)
            {
                return AuthenticateResult.Fail("User not found.");
            }

            // Create claims for the user
            var claims = new[] {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            await base.HandleChallengeAsync(properties);
        }
    }
}
