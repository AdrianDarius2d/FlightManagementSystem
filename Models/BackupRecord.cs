using System.ComponentModel.DataAnnotations;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Tracks the history of database backup operations.
    /// Satisfies REQ-54 (periodic backups), REQ-55 (manual backups), REQ-59 (log backup activities).
    /// </summary>
    public class BackupRecord
    {
        /// <summary>Primary key.</summary>
        public int Id { get; set; }

        /// <summary>Filename of the backup file.</summary>
        [Required]
        [Display(Name = "Backup File")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>Full path where the backup file is stored.</summary>
        [Display(Name = "Storage Path")]
        public string? StoragePath { get; set; }

        /// <summary>Size of the backup file in bytes.</summary>
        [Display(Name = "File Size (bytes)")]
        public long FileSizeBytes { get; set; }

        /// <summary>When the backup was initiated.</summary>
        [Display(Name = "Started At")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>When the backup completed.</summary>
        [Display(Name = "Completed At")]
        public DateTime? CompletedAt { get; set; }

        /// <summary>Status: Success | Failed | InProgress. Use ApplicationConstants.BACKUP_STATUS_* values.</summary>
        [Display(Name = "Status")]
        public string Status { get; set; } = "InProgress";

        /// <summary>Auto = scheduled; Manual = admin-initiated. Satisfies REQ-54 vs REQ-55.</summary>
        [Display(Name = "Type")]
        public string Type { get; set; } = "Auto";

        /// <summary>Who triggered the backup (IdentityUser.Id). Null for scheduled jobs.</summary>
        [Display(Name = "Initiated By")]
        public string? InitiatedByUserId { get; set; }

        /// <summary>Notes or error message if backup failed. Satisfies REQ-58 (verify integrity).</summary>
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        /// <summary>Whether backup integrity was verified after completion. Satisfies REQ-58.</summary>
        [Display(Name = "Integrity Verified")]
        public bool IntegrityVerified { get; set; } = false;
    }
}
