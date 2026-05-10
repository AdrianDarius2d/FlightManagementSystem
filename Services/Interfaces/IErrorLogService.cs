using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for recording and retrieving system error events.
    /// Satisfies REQ-68 (record errors) and REQ-69 (review error logs).
    /// </summary>
    public interface IErrorLogService
    {
        /// <summary>Records a system error to the database.</summary>
        Task LogErrorAsync(
            string message,
            string severity = "Error",
            string? stackTrace = null,
            string? requestPath = null,
            string? httpMethod = null,
            string? userId = null,
            string? ipAddress = null);

        /// <summary>Returns all error log entries, newest first.</summary>
        Task<IEnumerable<ErrorLog>> GetAllAsync();

        /// <summary>Returns unacknowledged (new) errors only.</summary>
        Task<IEnumerable<ErrorLog>> GetUnacknowledgedAsync();

        /// <summary>Marks an error as acknowledged by an admin.</summary>
        Task AcknowledgeAsync(int errorLogId);

        /// <summary>Returns count of unacknowledged errors.</summary>
        Task<int> GetUnacknowledgedCountAsync();
    }
}
