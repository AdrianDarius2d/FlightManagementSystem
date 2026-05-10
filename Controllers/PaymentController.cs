using FlightManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightManagementSystem.Controllers
{
    /// <summary>
    /// Handles payment processing for reservations (REQ-38 to REQ-45).
    /// </summary>
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IReservationService _reservationService;
        private readonly IFlightService _flightService;

        public PaymentController(
            IPaymentService paymentService,
            IReservationService reservationService,
            IFlightService flightService)
        {
            _paymentService = paymentService;
            _reservationService = reservationService;
            _flightService = flightService;
        }

        // REQ-38: show payment form with method selection
        public async Task<IActionResult> Payment(int reservationId)
        {
            var reservation = await _reservationService.GetByIdAsync(reservationId);
            if (reservation == null) return NotFound();

            // Calculate price based on class type
            var basePrice = reservation.Flight?.Price ?? 0;
            var multiplier = reservation.ClassType switch
            {
                "Business" => 2.0m,
                "First" => 3.5m,
                _ => 1.0m
            };

            ViewBag.Amount = Math.Round(basePrice * multiplier, 2);
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(int reservationId, string method, decimal amount)
        {
            try
            {
                var payment = await _paymentService.ProcessAsync(reservationId, method, amount);
                TempData["Success"] = $"Payment of ${amount:F2} confirmed. Transaction ID: {payment.TransactionId}";
                return RedirectToAction(nameof(Confirmation), new { reservationId });
            }
            catch (Exception)
            {
                // REQ-42: display error if payment fails
                TempData["Error"] = "Payment processing failed. Please try again.";
                return RedirectToAction(nameof(Payment), new { reservationId });
            }
        }

        // REQ-41: show payment confirmation
        public async Task<IActionResult> Confirmation(int reservationId)
        {
            var reservation = await _reservationService.GetByIdAsync(reservationId);
            if (reservation == null) return NotFound();
            var payment = await _paymentService.GetByReservationIdAsync(reservationId);
            ViewBag.Payment = payment;
            return View(reservation);
        }

        // REQ-45: Admin reviews all transactions
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllTransactions()
        {
            var payments = await _paymentService.GetAllAsync();
            return View(payments);
        }
    }
}
