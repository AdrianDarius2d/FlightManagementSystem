using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for reservation management.
    /// Satisfies REQ-19 to REQ-24 (reservation operations).
    /// </summary>
    public interface IReservationService
    {
        /// <summary>Returns all reservations. REQ-27.</summary>
        Task<IEnumerable<Reservation>> GetAllAsync();

        /// <summary>Returns all reservations for a specific passenger. REQ-27.</summary>
        Task<IEnumerable<Reservation>> GetByPassengerIdAsync(int passengerId);

        /// <summary>Returns all reservations for a specific flight — used for REQ-48 notifications.</summary>
        Task<IEnumerable<Reservation>> GetByFlightNumberAsync(string flightNumber);

        /// <summary>Returns a single reservation with all related data.</summary>
        Task<Reservation?> GetByIdAsync(int id);

        /// <summary>Creates a new confirmed reservation and issues a ticket. REQ-21.</summary>
        Task<Reservation> CreateAsync(int passengerId, string flightNumber, string classType);

        /// <summary>Cancels a reservation and notifies the passenger. REQ-24.</summary>
        Task CancelAsync(int id);

        /// <summary>Checks if a flight has available seats. REQ-23.</summary>
        Task<bool> CanReserveAsync(string flightNumber);
    }
}
