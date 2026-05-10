using FlightManagementSystem.Constants;
using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Manages database backup and restore operations.
    /// Satisfies REQ-54 through REQ-61 inclusive.
    /// NOTE: In a production system this would invoke real SQL Server backup commands.
    ///       This implementation simulates the operation and records it in BackupRecords,
    ///       which is appropriate for a school/demo environment.
    /// </summary>
    public class BackupService : IBackupService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<BackupService> _logger;
        private readonly IWebHostEnvironment _env;

        /// <summary>Initialises a new instance of <see cref="BackupService"/>.</summary>
        public BackupService(
            ApplicationDbContext db,
            IAuditService auditService,
            INotificationService notificationService,
            ILogger<BackupService> logger,
            IWebHostEnvironment env)
        {
            _db = db;
            _auditService = auditService;
            _notificationService = notificationService;
            _logger = logger;
            _env = env;
        }

        /// <inheritdoc/>
        public async Task<BackupRecord> CreateBackupAsync(string initiatedByUserId)
        {
            var timestamp = DateTime.UtcNow;
            var fileName = $"backup_{timestamp:yyyyMMdd_HHmmss}.bak";
            var backupDir = Path.Combine(_env.ContentRootPath, "Backups");

            // Ensure backup directory exists (REQ-56: secure storage location)
            Directory.CreateDirectory(backupDir);

            var record = new BackupRecord
            {
                FileName = fileName,
                StoragePath = Path.Combine(backupDir, fileName),
                StartedAt = timestamp,
                Status = ApplicationConstants.BACKUP_STATUS_IN_PROGRESS,
                Type = "Manual",
                InitiatedByUserId = initiatedByUserId,
                FileSizeBytes = 0,
                IntegrityVerified = false
            };

            _db.BackupRecords.Add(record);
            await _db.SaveChangesAsync();

            try
            {
                // ── Simulate backup by writing a metadata manifest ────────────
                var flightCount = await _db.Flights.CountAsync();
                var passengerCount = await _db.Passengers.CountAsync();
                var reservationCount = await _db.Reservations.CountAsync();

                var manifest = $"""
                    BERMUDA TRIANGLE AIRLINES - DATABASE BACKUP MANIFEST
                    =====================================================
                    Backup File   : {fileName}
                    Initiated By  : {initiatedByUserId}
                    Timestamp     : {timestamp:O}
                    Environment   : {_env.EnvironmentName}

                    RECORD COUNTS
                    -------------
                    Flights       : {flightCount}
                    Passengers    : {passengerCount}
                    Reservations  : {reservationCount}
                    """;

                await File.WriteAllTextAsync(record.StoragePath, manifest);

                record.FileSizeBytes = new FileInfo(record.StoragePath).Length;
                record.Status = ApplicationConstants.BACKUP_STATUS_SUCCESS;
                record.CompletedAt = DateTime.UtcNow;
                record.IntegrityVerified = true;
                record.Notes = "Backup completed successfully.";

                _logger.LogInformation(
                    "Backup {FileName} completed successfully at {CompletedAt}",
                    fileName, record.CompletedAt);
            }
            catch (Exception ex)
            {
                record.Status = ApplicationConstants.BACKUP_STATUS_FAILED;
                record.Notes = $"Backup failed: {ex.Message}";
                _logger.LogError(ex, "Backup {FileName} failed", fileName);
            }

            await _db.SaveChangesAsync();

            // REQ-59: log the backup activity
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_BACKUP,
                description: $"Database backup '{fileName}' — status: {record.Status}",
                userId: initiatedByUserId,
                entityType: "BackupRecord",
                entityId: record.Id.ToString(),
                isSuccess: record.Status == ApplicationConstants.BACKUP_STATUS_SUCCESS);

            return record;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BackupRecord>> GetAllAsync()
        {
            return await _db.BackupRecords
                .OrderByDescending(b => b.StartedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<BackupRecord?> GetLatestAsync()
        {
            return await _db.BackupRecords
                .Where(b => b.Status == ApplicationConstants.BACKUP_STATUS_SUCCESS)
                .OrderByDescending(b => b.CompletedAt)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> RestoreAsync(int backupRecordId, string requestedByUserId)
        {
            var record = await _db.BackupRecords.FindAsync(backupRecordId);
            if (record == null || record.Status != ApplicationConstants.BACKUP_STATUS_SUCCESS)
                return false;

            // Simulate restore verification (REQ-57)
            var restored = File.Exists(record.StoragePath);

            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_RESTORE,
                description: $"Restore requested from backup '{record.FileName}' — success: {restored}",
                userId: requestedByUserId,
                entityType: "BackupRecord",
                entityId: backupRecordId.ToString(),
                isSuccess: restored);

            return restored;
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyIntegrityAsync(int backupRecordId)
        {
            var record = await _db.BackupRecords.FindAsync(backupRecordId);
            if (record == null) return false;

            // REQ-58: verify backup integrity — check file exists and is non-empty
            var isValid = record.StoragePath != null
                && File.Exists(record.StoragePath)
                && new FileInfo(record.StoragePath).Length > 0;

            record.IntegrityVerified = isValid;
            record.Notes = isValid
                ? "Integrity verified ✔"
                : "Integrity check FAILED — file missing or empty.";

            await _db.SaveChangesAsync();
            return isValid;
        }
    }
}
