using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for reading and updating system configuration settings.
    /// Satisfies REQ-65 (configure system parameters) and REQ-72 (security policy configuration).
    /// Changes are automatically logged via IAuditService (REQ-76).
    /// </summary>
    public interface ISystemSettingService
    {
        /// <summary>Returns all system settings, grouped by category.</summary>
        Task<IEnumerable<SystemSetting>> GetAllAsync();

        /// <summary>Returns a single setting value by key.</summary>
        Task<string?> GetValueAsync(string key);

        /// <summary>Returns a typed boolean setting value.</summary>
        Task<bool> GetBoolAsync(string key, bool defaultValue = false);

        /// <summary>Returns a typed integer setting value.</summary>
        Task<int> GetIntAsync(string key, int defaultValue = 0);

        /// <summary>Updates a setting's value. Also logs the change (REQ-76).</summary>
        Task UpdateAsync(string key, string value, string updatedByUserId);

        /// <summary>Updates multiple settings in a single save. Satisfies REQ-65.</summary>
        Task UpdateManyAsync(Dictionary<string, string> settings, string updatedByUserId);
    }
}
