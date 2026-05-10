using FlightManagementSystem.Constants;
using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Business logic for reservation management.
    /// Satisfies REQ-19 to REQ-24, enforces seat capacity (REQ-23), and audits all operations.
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<ReservationService> _logger;

        /// <summary>Initialises a new instance of <see cref="ReservationService"/>.</summary>
        public ReservationService(
            ApplicationDbContext db,
            INotificationService notificationService,
            IAuditService auditService,
            ILogger<ReservationService> logger)
        {
            _db = db;
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _db.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Flight)
                .Include(r => r.Payment)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Reservation>> GetByPassengerIdAsync(int passengerId)
        {
            return await _db.Reservations
                .Include(r => r.Flight)
                .Include(r => r.Payment)
                .Where(r => r.PassengerId == passengerId)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Reservation>> GetByFlightNumberAsync(string flightNumber)
        {
            return await _db.Reservations
                .Include(r => r.Passenger)
                .Where(r => r.FlightId == flightNumber)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _db.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Flight)
                .Include(r => r.Payment)
                .Include(r => r.Ticket)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Reservation> CreateAsync(int passengerId, string flightNumber, string classType)
        {
            // REQ-23: enforce seat capacity before creating the reservation
            if (!await CanReserveAsync(flightNumber))
                throw new InvalidOperationException("No available seats on this flight.");

            var reservation = new Reservation
            {
                PassengerId = passengerId,
                FlightId = flightNumber,
                ClassType = classType,
                Status = ApplicationConstants.RESERVATION_STATUS_CONFIRMED,
                Date = DateTime.UtcNow
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            // Auto-issue a ticket (REQ-22: reservation must contain all required fields)
            var ticket = new Ticket
            {
                ReservationId = reservation.Id,
                SeatNumber = GenerateSeatNumber(),
                IssueDate = DateTime.UtcNow
            };
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            // REQ-80: audit log for reservation creation
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_CREATE,
                description: $"Reservation #{reservation.Id} created for passenger {passengerId} on flight {flightNumber} ({classType}).",
                entityType: "Reservation",
                entityId: reservation.Id.ToString());

            _logger.LogInformation(
                "Reservation {ReservationId} created for passenger {PassengerId} on flight {FlightNumber}",
                reservation.Id, passengerId, flightNumber);

            return reservation;
        }

        /// <inheritdoc/>
        public async Task CancelAsync(int id)
        {
            var reservation = await _db.Reservations
                .Include(r => r.Passenger)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Reservation not found.");

            reservation.Status = ApplicationConstants.RESERVATION_STATUS_CANCELLED;
            await _db.SaveChangesAsync();

            // REQ-47: notify the passenger about the cancellation
            if (!string.IsNullOrEmpty(reservation.Passenger?.IdentityUserId))
            {
                await _notificationService.SendAsync(
                    reservation.Passenger.IdentityUserId,
                    $"Your reservation #{reservation.Id} for flight {reservation.FlightId} has been cancelled.",
                    ApplicationConstants.NOTIFICATION_CANCELLATION);
            }

            // REQ-80: audit log for cancellation
            await _auditService.LogAsync(
                action: ApplicationConstants.AUDIT_ACTION_CANCEL,
                description: $"Reservation #{id} cancelled.",
                entityType: "Reservation",
                entityId: id.ToString());

            _logger.LogInformation("Reservation {ReservationId} cancelled", id);
        }

        /// <inheritdoc/>
        public async Task<bool> CanReserveAsync(string flightNumber)
        {
            var flight = await _db.Flights
                .Include(f => f.Reservations)
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

            if (flight == null || flight.Status == ApplicationConstants.FLIGHT_STATUS_CANCELLED)
                return false;

            return flight.AvailableSeats > 0;
        }

        /// <summary>Generates a random seat identifier (e.g. 14C).</summary>
        private static string GenerateSeatNumber()
        {
            var row = Random.Shared.Next(
                ApplicationConstants.SEAT_ROW_MIN,
                ApplicationConstants.SEAT_ROW_MAX);

            var col = (char)('A' + Random.Shared.Next(0, ApplicationConstants.SEAT_COLUMN_COUNT));
            return $"{row}{col}";
        }
    }
}
