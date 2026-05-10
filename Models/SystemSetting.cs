using System.ComponentModel.DataAnnotations;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Stores key-value system configuration parameters editable by admins.
    /// Satisfies REQ-65 (configure system parameters) and REQ-72 (configure security policies).
    /// Also supports REQ-76 (log configuration changes — tracked via ActivityLog).
    /// </summary>
    public class SystemSetting
    {
        /// <summary>Primary key — the setting key (use ApplicationConstants.SETTING_* values).</summary>
        [Key]
        [Display(Name = "Setting Key")]
        public string Key { get; set; } = string.Empty;

        /// <summary>Current value as a string (parsed by the consumer).</summary>
        [Required]
        [Display(Name = "Value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>Human-readable label shown in the admin UI.</summary>
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Category for grouping in the UI: Security | Notifications | Backup | General.</summary>
        [Display(Name = "Category")]
        public string Category { get; set; } = "General";

        /// <summary>Description of what this setting controls.</summary>
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Expected type for UI rendering: text | number | boolean | email.</summary>
        [Display(Name = "Input Type")]
        public string InputType { get; set; } = "text";

        /// <summary>When this setting was last modified.</summary>
        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>Who last modified this setting (IdentityUser.Id).</summary>
        [Display(Name = "Updated By")]
        public string? UpdatedByUserId { get; set; }
    }
}
