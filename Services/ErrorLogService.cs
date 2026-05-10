using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Records runtime system errors to the ErrorLog table and exposes them for admin review.
    /// Satisfies REQ-68 (record errors), REQ-69 (review logs), SRS §5.4.9 (exception handling).
    /// </summary>
    public class ErrorLogService : IErrorLogService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ErrorLogService> _logger;

        /// <summary>Initialises a new instance of <see cref="ErrorLogService"/>.</summary>
        public ErrorLogService(ApplicationDbContext db, ILogger<ErrorLogService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task LogErrorAsync(
            string message,
            string severity = "Error",
            string? stackTrace = null,
            string? requestPath = null,
            string? httpMethod = null,
            string? userId = null,
            string? ipAddress = null)
        {
            try
            {
                var entry = new ErrorLog
                {
                    Message = message,
                    Severity = severity,
                    StackTrace = stackTrace,
                    RequestPath = requestPath,
                    HttpMethod = httpMethod,
                    UserId = userId,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    IsAcknowledged = false
                };

                _db.ErrorLogs.Add(entry);
                await _db.SaveChangesAsync();

                // Also write to the .NET logging pipeline (SRS §5.4.25)
                _logger.LogError("System error recorded: {Message} | Path: {Path}", message, requestPath);
            }
            catch (Exception ex)
            {
                // Fallback — at minimum write to the .NET logger
                _logger.LogCritical(ex, "ErrorLogService failed to persist error: {Message}", message);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ErrorLog>> GetAllAsync()
        {
            return await _db.ErrorLogs
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ErrorLog>> GetUnacknowledgedAsync()
        {
            return await _db.ErrorLogs
                .Where(e => !e.IsAcknowledged)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AcknowledgeAsync(int errorLogId)
        {
            var entry = await _db.ErrorLogs.FindAsync(errorLogId);
            if (entry != null)
            {
                entry.IsAcknowledged = true;
                await _db.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetUnacknowledgedCountAsync()
        {
            return await _db.ErrorLogs.CountAsync(e => !e.IsAcknowledged);
        }
    }
}
