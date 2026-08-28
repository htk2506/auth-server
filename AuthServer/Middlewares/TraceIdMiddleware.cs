using System.Diagnostics;

namespace AuthServer.Middlewares
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string TraceIdHeader = "Trace-Id";

        public TraceIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            // Add trace ID to headers if not already in it
            if (!context.Response.Headers.ContainsKey(TraceIdHeader))
            {
                context.Response.Headers.TryAdd(TraceIdHeader, traceId);
            }

            await _next(context);
        }
    }
}
