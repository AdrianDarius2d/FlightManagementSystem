using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for recording user activity and audit trail events.
    /// Satisfies REQ-63 (activity logs), REQ-80 (audit logs), SRS §5.4.25-26 (logging).
    /// </summary>
    public interface IAuditService
    {
        /// <summary>Records a user action to the audit trail.</summary>
        Task LogAsync(
            string action,
            string description,
            string? userId = null,
            string? userEmail = null,
            string? entityType = null,
            string? entityId = null,
            string? ipAddress = null,
            bool isSuccess = true);

        /// <summary>Returns all activity log entries, newest first.</summary>
        Task<IEnumerable<ActivityLog>> GetAllAsync();

        /// <summary>Returns logs filtered by user or action type.</summary>
        Task<IEnumerable<ActivityLog>> SearchAsync(string? userId, string? action, DateTime? from, DateTime? to);

        /// <summary>Returns the N most recent log entries.</summary>
        Task<IEnumerable<ActivityLog>> GetRecentAsync(int count);
    }
}
