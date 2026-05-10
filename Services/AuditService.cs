using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Records all significant user and system events to the ActivityLog table.
    /// Satisfies REQ-63, REQ-66, REQ-76, REQ-80 and SRS §5.4.25-26.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuditService> _logger;

        /// <summary>Initialises a new instance of <see cref="AuditService"/>.</summary>
        public AuditService(ApplicationDbContext db, ILogger<AuditService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task LogAsync(
            string action,
            string description,
            string? userId = null,
            string? userEmail = null,
            string? entityType = null,
            string? entityId = null,
            string? ipAddress = null,
            bool isSuccess = true)
        {
            try
            {
                var entry = new ActivityLog
                {
                    Action = action,
                    Description = description,
                    UserId = userId,
                    UserEmail = userEmail,
                    EntityType = entityType,
                    EntityId = entityId,
                    IpAddress = ipAddress,
                    IsSuccess = isSuccess,
                    Timestamp = DateTime.UtcNow
                };

                _db.ActivityLogs.Add(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // SRS §5.4.25: log but do not let audit failure break the main flow
                _logger.LogError(ex, "Failed to write audit log entry for action {Action}", action);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ActivityLog>> GetAllAsync()
        {
            return await _db.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ActivityLog>> SearchAsync(
            string? userId, string? action, DateTime? from, DateTime? to)
        {
            var query = _db.ActivityLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(l => l.UserId == userId || l.UserEmail!.Contains(userId));

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(l => l.Action == action);

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value);

            return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ActivityLog>> GetRecentAsync(int count)
        {
            return await _db.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
