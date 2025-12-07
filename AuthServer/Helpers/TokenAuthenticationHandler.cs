using AuthServer.Database;
using AuthServer.Database.Models;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

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
            if (!isTokenValid || jwt == null) { return AuthenticateResult.Fail("Invalid token."); }

            // Verify token is for an active and unexpired session
            UserSession? session = _dbContext.UserSessions.FirstOrDefault(x => x.Id == Guid.Parse(jwt.Id) && DateTimeOffset.UtcNow < x.ExpiresAt);
            if (session == null) { return AuthenticateResult.Fail("No active session found."); }
            _dbContext.UserSessions.Entry(session).Reference(x => x.AppUser).Load();

            // Verify session belongs to same user as JWT
            Console.WriteLine("user: " + JsonSerializer.Serialize(session.AppUser));
            if (session.AppUser.Id.ToString() != jwt.Subject) { return AuthenticateResult.Fail("Session doesn't belong to user."); }

            // Create claims for the user
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, session.AppUser.Id.ToString()),    // user ID
                new Claim(ClaimTypes.Authentication, session.Id.ToString())             // session ID
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
