using FlightManagementSystem.Constants;
using FlightManagementSystem.Services.Interfaces;

namespace FlightManagementSystem.Middleware
{
    /// <summary>
    /// Intercepts POST requests to the Identity login endpoint and records
    /// successful and failed login attempts in the audit log.
    ///
    /// Satisfies:
    ///   REQ-66  — The system shall track login attempts.
    ///   REQ-80  — Maintain audit logs for security and compliance.
    ///   SRS §5.4.25 — Logging mechanisms for system events.
    /// </summary>
    public class LoginAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoginAuditMiddleware> _logger;

        /// <summary>Initialises a new instance of <see cref="LoginAuditMiddleware"/>.</summary>
        public LoginAuditMiddleware(RequestDelegate next, ILogger<LoginAuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>Processes the HTTP context, adding post-request login audit recording.</summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var isLoginPost =
                context.Request.Method == HttpMethods.Post &&
                context.Request.Path.StartsWithSegments("/Identity/Account/Login");

            await _next(context);

            if (!isLoginPost) return;

            try
            {
                var auditService = context.RequestServices.GetService<IAuditService>();
                if (auditService == null) return;

                var userId = context.User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var email = context.User.Identity?.Name ?? "unknown";
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                // If the user is now authenticated the login succeeded
                var succeeded = context.User.Identity?.IsAuthenticated == true;

                await auditService.LogAsync(
                    action: succeeded
                        ? ApplicationConstants.AUDIT_ACTION_LOGIN
                        : ApplicationConstants.AUDIT_ACTION_LOGIN_FAILED,
                    description: succeeded
                        ? $"User '{email}' logged in successfully."
                        : $"Failed login attempt for '{email}'.",
                    userId: userId,
                    userEmail: email,
                    ipAddress: ipAddress,
                    isSuccess: succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LoginAuditMiddleware failed to record login event");
            }
        }
    }
}
