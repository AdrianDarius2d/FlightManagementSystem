using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for flight management operations.
    /// Satisfies REQ-8 to REQ-13 (management) and REQ-30 to REQ-37 (search/filter/sort).
    /// </summary>
    public interface IFlightService
    {
        /// <summary>Returns all flights ordered by departure time.</summary>
        Task<IEnumerable<Flight>> GetAllFlightsAsync();

        /// <summary>
        /// Searches flights by source (REQ-30), destination (REQ-31), date (REQ-32),
        /// flight number (REQ-33), and sorts by the given criterion (REQ-35).
        /// </summary>
        Task<IEnumerable<Flight>> SearchFlightsAsync(
            string? source,
            string? destination,
            DateTime? date,
            string? flightNumber,
            string? sortBy = null);

        /// <summary>Returns a single flight by flight number, including reservations and passengers.</summary>
        Task<Flight?> GetByNumberAsync(string flightNumber);

        /// <summary>Persists a new flight record. REQ-8.</summary>
        Task CreateAsync(Flight flight);

        /// <summary>Updates an existing flight record. REQ-11.</summary>
        Task UpdateAsync(Flight flight);

        /// <summary>Deletes a flight record. REQ-12.</summary>
        Task DeleteAsync(string flightNumber);

        /// <summary>Returns true if a flight with the given number exists. REQ-13.</summary>
        Task<bool> ExistsAsync(string flightNumber);

        /// <summary>Returns the number of currently available seats on a flight.</summary>
        Task<int> GetAvailableSeatsAsync(string flightNumber);
    }
}
