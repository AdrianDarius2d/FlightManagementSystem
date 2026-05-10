using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Implements payment processing business logic (REQ-38 to REQ-45).
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notifications;

        public PaymentService(ApplicationDbContext db, INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public async Task<Payment> ProcessAsync(int reservationId, string method, decimal amount)
        {
            var reservation = await _db.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Flight)
                .FirstOrDefaultAsync(r => r.Id == reservationId)
                ?? throw new KeyNotFoundException("Reservation not found.");

            // REQ-39, REQ-40: record payment with date and method
            var payment = new Payment
            {
                ReservationId = reservationId,
                Amount = amount,
                Method = method,
                Date = DateTime.UtcNow,
                Status = "Completed",
                TransactionId = Guid.NewGuid().ToString()[..8].ToUpper()
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // REQ-41, REQ-46: confirm successful payment via notification
            if (!string.IsNullOrEmpty(reservation.Passenger?.IdentityUserId))
            {
                await _notifications.SendAsync(
                    reservation.Passenger.IdentityUserId,
                    $"Payment of ${amount:F2} confirmed for reservation #{reservationId} " +
                    $"on flight {reservation.Flight?.FlightNumber}. " +
                    $"Transaction ID: {payment.TransactionId}",
                    "Confirmation");
            }

            return payment;
        }

        public async Task<Payment?> GetByReservationIdAsync(int reservationId)
        {
            return await _db.Payments
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _db.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Passenger)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Flight)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }
    }
}
