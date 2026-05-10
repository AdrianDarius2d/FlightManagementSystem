using FlightManagementSystem.Constants;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightManagementSystem.Controllers
{
    /// <summary>
    /// System administration, monitoring, and configuration controller.
    /// Satisfies REQ-62 through REQ-80 (System Administration and Monitoring).
    /// All actions are restricted to the Admin role (REQ-79).
    /// </summary>
    [Authorize(Roles = ApplicationConstants.ROLE_ADMIN)]
    public class AdminController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly IReservationService _reservationService;
        private readonly IPassengerService _passengerService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IErrorLogService _errorLogService;
        private readonly IBackupService _backupService;
        private readonly ISystemSettingService _settingService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        /// <summary>Initialises a new instance of <see cref="AdminController"/>.</summary>
        public AdminController(
            IFlightService flightService,
            IReservationService reservationService,
            IPassengerService passengerService,
            IPaymentService paymentService,
            INotificationService notificationService,
            IAuditService auditService,
            IErrorLogService errorLogService,
            IBackupService backupService,
            ISystemSettingService settingService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _flightService = flightService;
            _reservationService = reservationService;
            _passengerService = passengerService;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _auditService = auditService;
            _errorLogService = errorLogService;
            _backupService = backupService;
            _settingService = settingService;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // ── REQ-62, REQ-64, REQ-71: Monitoring Dashboard ─────────────────────

        /// <summary>Main admin dashboard with system KPIs and recent activity. REQ-62, REQ-64, REQ-71.</summary>
        public async Task<IActionResult> Index()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            var reservations = await _reservationService.GetAllAsync();
            var passengers = await _passengerService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();
            var errorCount = await _errorLogService.GetUnacknowledgedCountAsync();
            var recentLogs = await _auditService.GetRecentAsync(ApplicationConstants.DASHBOARD_RECENT_ITEMS);
            var latestBackup = await _backupService.GetLatestAsync();

            ViewBag.TotalFlights = flights.Count();
            ViewBag.TotalReservations = reservations.Count();
            ViewBag.ActiveReservations = reservations.Count(r => r.Status == ApplicationConstants.RESERVATION_STATUS_CONFIRMED);
            ViewBag.TotalPassengers = passengers.Count();
            ViewBag.TotalRevenue = payments.Where(p => p.Status == ApplicationConstants.PAYMENT_STATUS_COMPLETED).Sum(p => p.Amount);
            ViewBag.RecentReservations = reservations.Take(ApplicationConstants.DASHBOARD_RECENT_ITEMS);
            ViewBag.FlightsToday = flights.Count(f => f.DepartureTime.Date == DateTime.Today);
            ViewBag.UnacknowledgedErrors = errorCount;
            ViewBag.RecentActivityLogs = recentLogs;
            ViewBag.LatestBackup = latestBackup;
            ViewBag.MaintenanceMode = await _settingService.GetBoolAsync(ApplicationConstants.SETTING_MAINTENANCE_MODE);

            return View();
        }

        // ── REQ-67, REQ-74, REQ-75: User Account Management ──────────────────

        /// <summary>Lists all registered users with their roles. REQ-67, REQ-74, REQ-75.</summary>
        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = userRoles;
            ViewBag.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(users);
        }

        /// <summary>Locks or unlocks a user account. REQ-67.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var adminId = _userManager.GetUserId(User);
            var isCurrentlyLocked = await _userManager.IsLockedOutAsync(user);

            if (isCurrentlyLocked)
                await _userManager.SetLockoutEndDateAsync(user, null);
            else
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            var action = isCurrentlyLocked ? "unlocked" : "locked";
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_ACCOUNT_LOCKED,
                description: $"Admin {action} account for user '{user.Email}'.",
                userId: adminId,
                entityType: "IdentityUser",
                entityId: userId);

            TempData["Success"] = $"User account {action}.";
            return RedirectToAction(nameof(Users));
        }

        /// <summary>Assigns a role to a user. REQ-74.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var adminId = _userManager.GetUserId(User);
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            // REQ-76, REQ-80: log role change
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_ROLE_CHANGE,
                description: $"Role changed to '{role}' for user '{user.Email}' (was: {string.Join(", ", currentRoles)}).",
                userId: adminId,
                entityType: "IdentityUser",
                entityId: userId);

            TempData["Success"] = $"Role '{role}' assigned to {user.Email}.";
            return RedirectToAction(nameof(Users));
        }

        /// <summary>Resets a user's password. REQ-75.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var adminId = _userManager.GetUserId(User);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                await _auditService.LogAsync(
                    action: ApplicationConstants.AUDIT_ACTION_PASSWORD_RESET,
                    description: $"Password reset by admin for user '{user.Email}'.",
                    userId: adminId,
                    entityType: "IdentityUser",
                    entityId: userId);

                TempData["Success"] = "Password reset successfully.";
            }
            else
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Users));
        }

        // ── REQ-25, REQ-26, REQ-28, REQ-29: Reports ──────────────────────────

        /// <summary>Generates operational reports on flights, revenue, and reservations. REQ-25 to REQ-29.</summary>
        public async Task<IActionResult> Reports()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            var reservations = await _reservationService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();

            ViewBag.FlightStats = flights.Select(f => new
            {
                f.FlightNumber,
                f.Source,
                f.Destination,
                f.DepartureTime,
                f.Capacity,
                Booked = f.BookedSeats,
                Revenue = reservations
                    .Where(r => r.FlightId == f.FlightNumber && r.Payment != null)
                    .Sum(r => r.Payment!.Amount)
            });

            ViewBag.TotalRevenue = payments.Where(p => p.Status == ApplicationConstants.PAYMENT_STATUS_COMPLETED).Sum(p => p.Amount);

            ViewBag.RevenueByMethod = payments
                .GroupBy(p => p.Method)
                .Select(g => new { Method = g.Key, Total = g.Sum(p => p.Amount) });

            return View(reservations);
        }

        // ── REQ-63, REQ-80: Activity / Audit Logs ────────────────────────────

        /// <summary>Displays user activity and audit trail log. REQ-63, REQ-80.</summary>
        public async Task<IActionResult> ActivityLogs(
            string? userId, string? action, DateTime? from, DateTime? to)
        {
            var logs = await _auditService.SearchAsync(userId, action, from, to);

            ViewBag.FilterUserId = userId;
            ViewBag.FilterAction = action;
            ViewBag.FilterFrom = from?.ToString("yyyy-MM-dd");
            ViewBag.FilterTo = to?.ToString("yyyy-MM-dd");

            return View(logs);
        }

        // ── REQ-68, REQ-69: Error Logs ────────────────────────────────────────

        /// <summary>Displays system error log for admin review. REQ-68, REQ-69.</summary>
        public async Task<IActionResult> ErrorLogs()
        {
            var errors = await _errorLogService.GetAllAsync();
            ViewBag.UnacknowledgedCount = await _errorLogService.GetUnacknowledgedCountAsync();
            return View(errors);
        }

        /// <summary>Marks an error log entry as acknowledged by an admin. REQ-69.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeError(int id)
        {
            await _errorLogService.AcknowledgeAsync(id);
            TempData["Success"] = "Error acknowledged.";
            return RedirectToAction(nameof(ErrorLogs));
        }

        // ── REQ-54 to REQ-61: Backup Management ──────────────────────────────

        /// <summary>Displays backup history and controls. REQ-54 to REQ-61.</summary>
        public async Task<IActionResult> Backup()
        {
            var backups = await _backupService.GetAllAsync();
            ViewBag.LatestBackup = await _backupService.GetLatestAsync();
            return View(backups);
        }

        /// <summary>Initiates a manual database backup. REQ-55.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBackup()
        {
            var userId = _userManager.GetUserId(User)!;
            var record = await _backupService.CreateBackupAsync(userId);

            TempData[record.Status == ApplicationConstants.BACKUP_STATUS_SUCCESS ? "Success" : "Error"] =
                record.Status == ApplicationConstants.BACKUP_STATUS_SUCCESS
                    ? $"Backup '{record.FileName}' created successfully."
                    : $"Backup failed: {record.Notes}";

            return RedirectToAction(nameof(Backup));
        }

        /// <summary>Verifies the integrity of a backup file. REQ-58.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyBackup(int id)
        {
            var ok = await _backupService.VerifyIntegrityAsync(id);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Backup integrity verified successfully."
                : "Backup integrity check FAILED — file may be corrupt or missing.";

            return RedirectToAction(nameof(Backup));
        }

        /// <summary>Restores from a selected backup. REQ-57.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreBackup(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var ok = await _backupService.RestoreAsync(id, userId);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Restore simulation completed. In production this would apply the backup."
                : "Restore failed — backup file not found.";

            return RedirectToAction(nameof(Backup));
        }

        // ── REQ-65, REQ-72, REQ-76: System Settings & Security Policy ────────

        /// <summary>Displays configurable system settings. REQ-65, REQ-72.</summary>
        public async Task<IActionResult> Settings()
        {
            var settings = await _settingService.GetAllAsync();
            return View(settings);
        }

        /// <summary>Saves updated system settings. REQ-65, REQ-76 (changes are logged).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(Dictionary<string, string> settings)
        {
            var userId = _userManager.GetUserId(User)!;
            await _settingService.UpdateManyAsync(settings, userId);
            TempData["Success"] = "Settings saved. Changes have been recorded in the audit log.";
            return RedirectToAction(nameof(Settings));
        }

        // ── REQ-70, REQ-78: Maintenance Mode ─────────────────────────────────

        /// <summary>Toggles system maintenance mode. REQ-70, REQ-78.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMaintenanceMode()
        {
            var userId = _userManager.GetUserId(User)!;
            var current = await _settingService.GetBoolAsync(ApplicationConstants.SETTING_MAINTENANCE_MODE);
            await _settingService.UpdateAsync(
                ApplicationConstants.SETTING_MAINTENANCE_MODE,
                (!current).ToString(),
                userId);

            TempData["Success"] = !current
                ? "Maintenance mode ENABLED. Users will see a maintenance notice."
                : "Maintenance mode DISABLED. System is back online.";

            return RedirectToAction(nameof(Index));
        }

        // ── REQ-73: System Health Report ─────────────────────────────────────

        /// <summary>Generates a system health summary report. REQ-73.</summary>
        public async Task<IActionResult> SystemHealth()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            var reservations = await _reservationService.GetAllAsync();
            var payments = await _paymentService.GetAllAsync();
            var passengers = await _passengerService.GetAllAsync();
            var latestBackup = await _backupService.GetLatestAsync();
            var errorCount = await _errorLogService.GetUnacknowledgedCountAsync();
            var settings = await _settingService.GetAllAsync();
            var userCount = _userManager.Users.Count();

            ViewBag.TotalUsers = userCount;
            ViewBag.TotalFlights = flights.Count();
            ViewBag.ScheduledFlights = flights.Count(f => f.Status == ApplicationConstants.FLIGHT_STATUS_SCHEDULED);
            ViewBag.TotalPassengers = passengers.Count();
            ViewBag.TotalReservations = reservations.Count();
            ViewBag.ConfirmedReservations = reservations.Count(r => r.Status == ApplicationConstants.RESERVATION_STATUS_CONFIRMED);
            ViewBag.TotalPayments = payments.Count(p => p.Status == ApplicationConstants.PAYMENT_STATUS_COMPLETED);
            ViewBag.TotalRevenue = payments.Where(p => p.Status == ApplicationConstants.PAYMENT_STATUS_COMPLETED).Sum(p => p.Amount);
            ViewBag.UnacknowledgedErrors = errorCount;
            ViewBag.LatestBackup = latestBackup;
            ViewBag.MaintenanceMode = await _settingService.GetBoolAsync(ApplicationConstants.SETTING_MAINTENANCE_MODE);
            ViewBag.Settings = settings;

            return View();
        }

        // ── REQ-52, REQ-77: Notifications ────────────────────────────────────

        /// <summary>Sends a system-wide notification to all users. REQ-52, REQ-77.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendSystemAlert(string message)
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
                await _notificationService.SendAsync(user.Id, message, ApplicationConstants.NOTIFICATION_SYSTEM_ALERT);

            var adminId = _userManager.GetUserId(User)!;
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_CREATE,
                description: $"System alert sent to {users.Count} users: \"{message}\"",
                userId: adminId,
                entityType: "Notification");

            TempData["Success"] = $"Alert sent to {users.Count} users.";
            return RedirectToAction(nameof(Index));
        }
    }
}
