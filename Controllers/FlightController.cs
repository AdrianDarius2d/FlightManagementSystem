using FlightManagementSystem.Constants;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightManagementSystem.Controllers
{
    /// <summary>
    /// Handles flight listing, searching, and CRUD for authorised staff/admin.
    /// Satisfies REQ-8 to REQ-13 (flight management) and REQ-30 to REQ-37 (search and filtering).
    /// </summary>
    public class FlightController : Controller
    {
        private readonly IFlightService _flightService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IReservationService _reservationService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<FlightController> _logger;

        /// <summary>Initialises a new instance of <see cref="FlightController"/>.</summary>
        public FlightController(
            IFlightService flightService,
            INotificationService notificationService,
            IAuditService auditService,
            IReservationService reservationService,
            UserManager<IdentityUser> userManager,
            ILogger<FlightController> logger)
        {
            _flightService = flightService;
            _notificationService = notificationService;
            _auditService = auditService;
            _reservationService = reservationService;
            _userManager = userManager;
            _logger = logger;
        }

        // ── REQ-30 to REQ-37: Search / List Flights (open to all users) ──────

        /// <summary>
        /// Displays and filters the flight list.
        /// Supports search by source (REQ-30), destination (REQ-31), date (REQ-32),
        /// flight number (REQ-33), sorting (REQ-35), no-result message (REQ-36).
        /// </summary>
        public async Task<IActionResult> Index(
            string? source,
            string? destination,
            DateTime? date,
            string? flightNumber,
            string? sortBy)
        {
            var flights = await _flightService.SearchFlightsAsync(
                source, destination, date, flightNumber, sortBy);

            ViewBag.Source = source;
            ViewBag.Destination = destination;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");
            ViewBag.FlightNumber = flightNumber;
            ViewBag.SortBy = sortBy;
            ViewBag.NoResults = !flights.Any();

            return View(flights);
        }

        /// <summary>Shows full details for a single flight.</summary>
        public async Task<IActionResult> Details(string id)
        {
            var flight = await _flightService.GetByNumberAsync(id);
            if (flight == null) return NotFound();
            return View(flight);
        }

        // ── REQ-8, REQ-9, REQ-10, REQ-13: Create Flight (Staff / Admin only) ──

        /// <summary>Shows the create-flight form. REQ-8.</summary>
        [Authorize(Roles = $"{ApplicationConstants.ROLE_ADMIN},{ApplicationConstants.ROLE_STAFF}")]
        public IActionResult Create() => View();

        /// <summary>Processes the create-flight form. REQ-9, REQ-10, REQ-13.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApplicationConstants.ROLE_ADMIN},{ApplicationConstants.ROLE_STAFF}")]
        public async Task<IActionResult> Create(Flight flight)
        {
            // REQ-13: prevent duplicate flight numbers
            if (await _flightService.ExistsAsync(flight.FlightNumber))
                ModelState.AddModelError("FlightNumber", "A flight with this number already exists.");

            if (!ModelState.IsValid) return View(flight);

            await _flightService.CreateAsync(flight);

            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_CREATE,
                description: $"Flight '{flight.FlightNumber}' ({flight.Source}→{flight.Destination}) created.",
                userId: userId,
                entityType: "Flight",
                entityId: flight.FlightNumber);

            _logger.LogInformation("Flight {FlightNumber} created by user {UserId}",
                flight.FlightNumber, userId);

            TempData["Success"] = $"Flight {flight.FlightNumber} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── REQ-11: Update Flight (Staff / Admin only) ────────────────────────

        /// <summary>Shows the edit-flight form. REQ-11.</summary>
        [Authorize(Roles = $"{ApplicationConstants.ROLE_ADMIN},{ApplicationConstants.ROLE_STAFF}")]
        public async Task<IActionResult> Edit(string id)
        {
            var flight = await _flightService.GetByNumberAsync(id);
            if (flight == null) return NotFound();
            return View(flight);
        }

        /// <summary>
        /// Processes the edit-flight form. REQ-11.
        /// Also notifies all passengers on the flight if the schedule changed (REQ-48).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApplicationConstants.ROLE_ADMIN},{ApplicationConstants.ROLE_STAFF}")]
        public async Task<IActionResult> Edit(string id, Flight flight)
        {
            if (id != flight.FlightNumber) return BadRequest();
            if (!ModelState.IsValid) return View(flight);

            // Capture old values before updating to detect schedule changes (REQ-48)
            var existing = await _flightService.GetByNumberAsync(id);
            var scheduleChanged = existing != null && (
                existing.DepartureTime != flight.DepartureTime ||
                existing.ArrivalTime != flight.ArrivalTime ||
                existing.Status != flight.Status);

            await _flightService.UpdateAsync(flight);

            var userId = _userManager.GetUserId(User);
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_UPDATE,
                description: $"Flight '{flight.FlightNumber}' updated. Schedule changed: {scheduleChanged}.",
                userId: userId,
                entityType: "Flight",
                entityId: flight.FlightNumber);

            // REQ-48: notify all confirmed passengers if the schedule changed
            if (scheduleChanged)
            {
                var reservations = await _reservationService.GetByFlightNumberAsync(flight.FlightNumber);
                foreach (var r in reservations.Where(r =>
                    r.Status == ApplicationConstants.RESERVATION_STATUS_CONFIRMED &&
                    !string.IsNullOrEmpty(r.Passenger?.IdentityUserId)))
                {
                    await _notificationService.SendAsync(
                        r.Passenger!.IdentityUserId!,
                        $"⚠️ Your flight {flight.FlightNumber} schedule has been updated. " +
                        $"New departure: {flight.DepartureTime:dd MMM yyyy HH:mm}. " +
                        $"Status: {flight.Status}.",
                        ApplicationConstants.NOTIFICATION_FLIGHT_CHANGE);
                }

                TempData["Success"] = $"Flight {flight.FlightNumber} updated. " +
                    $"Passengers have been notified of the schedule change.";
            }
            else
            {
                TempData["Success"] = $"Flight {flight.FlightNumber} updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── REQ-12: Delete Flight (Staff / Admin only) ────────────────────────

        /// <summary>Shows the delete confirmation page. REQ-12.</summary>
        [Authorize(Roles = $"{ApplicationConstants.ROLE_ADMIN},{ApplicationConstants.ROLE_STAFF}")]
        public async Task<IActionResult> Delete(string id)
        {
            var flight = await _flightService.GetByNumberAsync(id);
            if (flight == null) return NotFound();
            return View(flight);
        }

        /// <summary>Confirms and executes flight deletion. REQ-12.</summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{ApplicationConstants.ROLE_ADMIN},{ApplicationConstants.ROLE_STAFF}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var userId = _userManager.GetUserId(User);
            await _flightService.DeleteAsync(id);

            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_DELETE,
                description: $"Flight '{id}' deleted.",
                userId: userId,
                entityType: "Flight",
                entityId: id);

            TempData["Success"] = "Flight deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ── REQ-26: Passenger list per flight ─────────────────────────────────

        /// <summary>Shows all passengers booked on a specific flight. REQ-26.</summary>
        [Authorize(Roles = $"{ApplicationConstants.ROLE_ADMIN},{ApplicationConstants.ROLE_STAFF}")]
        public async Task<IActionResult> PassengerList(string id)
        {
            var flight = await _flightService.GetByNumberAsync(id);
            if (flight == null) return NotFound();
            return View(flight);
        }
    }
}
