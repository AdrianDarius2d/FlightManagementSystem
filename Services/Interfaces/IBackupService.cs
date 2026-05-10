using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for database backup and restore operations.
    /// Satisfies REQ-54 (periodic backups), REQ-55 (manual backups), REQ-56 (secure storage),
    /// REQ-57 (restoration), REQ-58 (integrity verification), REQ-59 (activity logging),
    /// REQ-60 (access protection), REQ-61 (minimal downtime).
    /// </summary>
    public interface IBackupService
    {
        /// <summary>Initiates a manual backup. Satisfies REQ-55.</summary>
        Task<BackupRecord> CreateBackupAsync(string initiatedByUserId);

        /// <summary>Returns all backup records, newest first. Satisfies REQ-59.</summary>
        Task<IEnumerable<BackupRecord>> GetAllAsync();

        /// <summary>Returns the most recent successful backup.</summary>
        Task<BackupRecord?> GetLatestAsync();

        /// <summary>Simulates restoring from a backup record. Satisfies REQ-57.</summary>
        Task<bool> RestoreAsync(int backupRecordId, string requestedByUserId);

        /// <summary>Verifies the integrity of a backup file. Satisfies REQ-58.</summary>
        Task<bool> VerifyIntegrityAsync(int backupRecordId);
    }
}
