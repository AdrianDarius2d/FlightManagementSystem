using System.ComponentModel.DataAnnotations;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Records user actions within the system for audit trail purposes.
    /// Satisfies REQ-63 (user activity logs) and REQ-80 (audit logs for security/compliance).
    /// </summary>
    public class ActivityLog
    {
        /// <summary>Primary key.</summary>
        public int Id { get; set; }

        /// <summary>IdentityUser.Id of the user who performed the action. Null for system events.</summary>
        public string? UserId { get; set; }

        /// <summary>Email of the user for readable display.</summary>
        [Display(Name = "User")]
        public string? UserEmail { get; set; }

        /// <summary>Action type - use ApplicationConstants.AUDIT_ACTION_* values.</summary>
        [Required]
        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty;

        /// <summary>Human-readable description of what happened.</summary>
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Entity type affected (e.g. "Flight", "Reservation", "User").</summary>
        [Display(Name = "Entity")]
        public string? EntityType { get; set; }

        /// <summary>ID of the entity affected.</summary>
        [Display(Name = "Entity ID")]
        public string? EntityId { get; set; }

        /// <summary>IP address of the user.</summary>
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        /// <summary>Timestamp of the action.</summary>
        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>Whether this was a successful operation.</summary>
        public bool IsSuccess { get; set; } = true;
    }
}
