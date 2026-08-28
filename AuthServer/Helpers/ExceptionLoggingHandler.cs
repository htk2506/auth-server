using Microsoft.AspNetCore.Diagnostics;

namespace AuthServer.Helpers
{
    public class ExceptionLoggingHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionLoggingHandler> _logger;

        public ExceptionLoggingHandler(ILogger<ExceptionLoggingHandler> logger)
        {
            _logger = logger;
        }

        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Error occurred. Exception type: {ExceptionType} and exception message: {ExceptionMessage}.", exception.GetType(), exception.Message);

            // Return false to continue with the default behavior
            return ValueTask.FromResult(false);
        }
    }
}