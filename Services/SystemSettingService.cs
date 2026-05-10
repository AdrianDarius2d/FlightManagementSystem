using FlightManagementSystem.Constants;
using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Manages system configuration key-value pairs stored in the database.
    /// Satisfies REQ-65 (configure parameters), REQ-72 (security policies), REQ-76 (log changes).
    /// </summary>
    public class SystemSettingService : ISystemSettingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;
        private readonly ILogger<SystemSettingService> _logger;

        /// <summary>Initialises a new instance of <see cref="SystemSettingService"/>.</summary>
        public SystemSettingService(
            ApplicationDbContext db,
            IAuditService auditService,
            ILogger<SystemSettingService> logger)
        {
            _db = db;
            _auditService = auditService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SystemSetting>> GetAllAsync()
        {
            return await _db.SystemSettings
                .OrderBy(s => s.Category)
                .ThenBy(s => s.DisplayName)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<string?> GetValueAsync(string key)
        {
            var setting = await _db.SystemSettings.FindAsync(key);
            return setting?.Value;
        }

        /// <inheritdoc/>
        public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
        {
            var value = await GetValueAsync(key);
            return value != null && bool.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <inheritdoc/>
        public async Task<int> GetIntAsync(string key, int defaultValue = 0)
        {
            var value = await GetValueAsync(key);
            return value != null && int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(string key, string value, string updatedByUserId)
        {
            var setting = await _db.SystemSettings.FindAsync(key);
            if (setting == null) return;

            var oldValue = setting.Value;
            setting.Value = value;
            setting.LastUpdated = DateTime.UtcNow;
            setting.UpdatedByUserId = updatedByUserId;

            await _db.SaveChangesAsync();

            // REQ-76: log every configuration change
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_CONFIG_CHANGE,
                description: $"Setting '{setting.DisplayName}' changed from '{oldValue}' to '{value}'",
                userId: updatedByUserId,
                entityType: "SystemSetting",
                entityId: key);

            _logger.LogInformation(
                "System setting '{Key}' updated to '{Value}' by user {UserId}",
                key, value, updatedByUserId);
        }

        /// <inheritdoc/>
        public async Task UpdateManyAsync(Dictionary<string, string> settings, string updatedByUserId)
        {
            foreach (var kvp in settings)
                await UpdateAsync(kvp.Key, kvp.Value, updatedByUserId);
        }
    }
}
