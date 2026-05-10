using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightManagementSystem.Controllers
{
    /// <summary>
    /// Manages flight reservations (REQ-19 to REQ-24).
    /// Passengers book/cancel their own flights; staff can see all reservations.
    /// </summary>
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IPassengerService _passengerService;
        private readonly IFlightService _flightService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationController(
            IReservationService reservationService,
            IPassengerService passengerService,
            IFlightService flightService,
            INotificationService notificationService,
            UserManager<IdentityUser> userManager)
        {
            _reservationService = reservationService;
            _passengerService = passengerService;
            _flightService = flightService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // REQ-27: My reservations (passenger view)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var passenger = await _passengerService.GetByIdentityUserIdAsync(userId!);

            if (passenger == null)
                return RedirectToAction("Create", "Passenger", new { returnUrl = Url.Action("Index") });

            var reservations = await _reservationService.GetByPassengerIdAsync(passenger.Id);
            return View(reservations);
        }

        // Staff / Admin view all reservations
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> All()
        {
            var reservations = await _reservationService.GetAllAsync();
            return View(reservations);
        }

        // REQ-21: Create a reservation for a selected flight
        public async Task<IActionResult> Create(string flightNumber)
        {
            var flight = await _flightService.GetByNumberAsync(flightNumber);
            if (flight == null) return NotFound();

            // REQ-23: check seat availability before showing the form
            if (!await _reservationService.CanReserveAsync(flightNumber))
            {
                TempData["Error"] = "Sorry, this flight is fully booked.";
                return RedirectToAction("Details", "Flight", new { id = flightNumber });
            }

            var userId = _userManager.GetUserId(User);
            var passenger = await _passengerService.GetByIdentityUserIdAsync(userId!);

            if (passenger == null)
            {
                TempData["Info"] = "Please complete your passenger profile before booking.";
                return RedirectToAction("Create", "Passenger",
                    new { returnUrl = Url.Action("Create", new { flightNumber }) });
            }

            ViewBag.Flight = flight;
            ViewBag.Passenger = passenger;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string flightNumber, string classType)
        {
            var userId = _userManager.GetUserId(User);
            var passenger = await _passengerService.GetByIdentityUserIdAsync(userId!);

            if (passenger == null)
                return RedirectToAction("Create", "Passenger");

            try
            {
                var reservation = await _reservationService.CreateAsync(passenger.Id, flightNumber, classType);

                // REQ-46: send confirmation notification
                await _notificationService.SendAsync(
                    userId!,
                    $"Reservation #{reservation.Id} confirmed for flight {flightNumber} ({classType} class).",
                    "Confirmation");

                TempData["Success"] = $"Reservation #{reservation.Id} created successfully!";
                return RedirectToAction("Payment", "Payment", new { reservationId = reservation.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Flight", new { id = flightNumber });
            }
        }

        // REQ-24: Cancel a reservation
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null) return NotFound();

            // Ensure passenger can only cancel their own reservations
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var userId = _userManager.GetUserId(User);
                var passenger = await _passengerService.GetByIdentityUserIdAsync(userId!);
                if (passenger == null || reservation.PassengerId != passenger.Id)
                    return Forbid();
            }

            return View(reservation);
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            await _reservationService.CancelAsync(id);
            TempData["Success"] = $"Reservation #{id} has been cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // REQ-22: Reservation details
        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            if (reservation == null) return NotFound();
            return View(reservation);
        }
    }
}
