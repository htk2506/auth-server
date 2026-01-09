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
    public class SessionJwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtService _jwtService;
        private readonly AppDbContext _dbContext;

        public SessionJwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            JwtService jwtService,
            AppDbContext dbContext
        ) : base(options, logger, encoder)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Get the auth header
            string? authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("Invalid authorization header.");
            }

            // Extract the JWT
            string token = authHeader.Substring("Bearer ".Length).Trim();

            // Validate the JWT 
            bool isTokenValid = _jwtService.ValidateJwt(token, out JwtSecurityToken? jwt);
            if (!isTokenValid || jwt == null) { return AuthenticateResult.Fail("Invalid token."); }

            // Verify JWT is for an active and unexpired session
            UserSession? session = _dbContext.UserSessions.FirstOrDefault(x => x.Id == Guid.Parse(jwt.Id) && DateTimeOffset.UtcNow < x.ExpiresAt);
            if (session == null) { return AuthenticateResult.Fail("No active session found."); }
            _dbContext.UserSessions.Entry(session).Reference(x => x.AppUser).Load();

            // Verify session belongs to same user as JWT
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
