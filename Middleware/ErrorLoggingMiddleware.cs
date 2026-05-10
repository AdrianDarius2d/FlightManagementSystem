using FlightManagementSystem.Services.Interfaces;

namespace FlightManagementSystem.Middleware
{
    /// <summary>
    /// ASP.NET Core middleware that intercepts all unhandled exceptions,
    /// records them via <see cref="IErrorLogService"/>, and returns a safe
    /// error response to the user without exposing internal stack traces.
    ///
    /// Satisfies:
    ///   REQ-68  — The system shall record system errors.
    ///   REQ-77  — The system shall notify administrators of critical failures.
    ///   SRS §5.4.9  — Structured exception handling.
    ///   SRS §5.4.10 — Internal error details shall not be exposed to end users.
    /// </summary>
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorLoggingMiddleware> _logger;

        /// <summary>Initialises a new instance of <see cref="ErrorLoggingMiddleware"/>.</summary>
        public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>Processes the HTTP request, catching and logging any unhandled exception.</summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await RecordErrorAsync(context, ex);

                // SRS §5.4.10: redirect to safe error page — never expose stack trace to users
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Home/Error");
                }
            }
        }

        /// <summary>Persists the error details using a scoped <see cref="IErrorLogService"/>.</summary>
        private static async Task RecordErrorAsync(HttpContext context, Exception ex)
        {
            try
            {
                // Resolve scoped service from the current request scope (SRS §5.4.24)
                var errorLogService = context.RequestServices.GetService<IErrorLogService>();
                if (errorLogService == null) return;

                var userId = context.User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await errorLogService.LogErrorAsync(
                    message: ex.Message,
                    severity: ex is InvalidOperationException ? "Warning" : "Error",
                    stackTrace: ex.StackTrace,
                    requestPath: context.Request.Path,
                    httpMethod: context.Request.Method,
                    userId: userId,
                    ipAddress: context.Connection.RemoteIpAddress?.ToString());
            }
            catch
            {
                // Swallow any secondary exception — logging must never crash the app
            }
        }
    }
}
