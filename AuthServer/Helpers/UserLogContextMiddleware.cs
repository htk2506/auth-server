using Serilog.Context;
using System.Security.Claims;

namespace AuthServer.Helpers
{
    public class UserLogContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserLogContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "" : "";

            using (LogContext.PushProperty("UserId", userId))
            {
                await _next(context);
            }
        }
    }
}
