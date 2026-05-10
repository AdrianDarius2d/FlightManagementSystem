using FlightManagementSystem.Constants;
using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Business logic for flight management.
    /// Satisfies REQ-8 to REQ-13 (CRUD) and REQ-30 to REQ-37 (search and filtering).
    /// </summary>
    public class FlightService : IFlightService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<FlightService> _logger;

        /// <summary>Initialises a new instance of <see cref="FlightService"/>.</summary>
        public FlightService(ApplicationDbContext db, ILogger<FlightService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            return await _db.Flights
                .Include(f => f.Reservations)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Flight>> SearchFlightsAsync(
            string? source,
            string? destination,
            DateTime? date,
            string? flightNumber,
            string? sortBy = null)
        {
            var query = _db.Flights.Include(f => f.Reservations).AsQueryable();

            // REQ-30: filter by departure location
            if (!string.IsNullOrWhiteSpace(source))
                query = query.Where(f => f.Source.Contains(source));

            // REQ-31: filter by destination
            if (!string.IsNullOrWhiteSpace(destination))
                query = query.Where(f => f.Destination.Contains(destination));

            // REQ-32: filter by departure date
            if (date.HasValue)
                query = query.Where(f => f.DepartureTime.Date == date.Value.Date);

            // REQ-33: filter by flight number
            if (!string.IsNullOrWhiteSpace(flightNumber))
                query = query.Where(f => f.FlightNumber.Contains(flightNumber));

            // REQ-35: sort results
            query = sortBy switch
            {
                "price"      => query.OrderBy(f => f.Price),
                "price_desc" => query.OrderByDescending(f => f.Price),
                "arrival"    => query.OrderBy(f => f.ArrivalTime),
                _            => query.OrderBy(f => f.DepartureTime)
            };

            return await query
                .Take(ApplicationConstants.MAX_SEARCH_RESULTS)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Flight?> GetByNumberAsync(string flightNumber)
        {
            return await _db.Flights
                .Include(f => f.Reservations)
                    .ThenInclude(r => r.Passenger)
                .Include(f => f.Reservations)
                    .ThenInclude(r => r.Payment)
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);
        }

        /// <inheritdoc/>
        public async Task CreateAsync(Flight flight)
        {
            _db.Flights.Add(flight);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Flight {FlightNumber} created", flight.FlightNumber);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Flight flight)
        {
            _db.Flights.Update(flight);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Flight {FlightNumber} updated", flight.FlightNumber);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string flightNumber)
        {
            var flight = await _db.Flights.FindAsync(flightNumber);
            if (flight != null)
            {
                _db.Flights.Remove(flight);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Flight {FlightNumber} deleted", flightNumber);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string flightNumber)
        {
            return await _db.Flights.AnyAsync(f => f.FlightNumber == flightNumber);
        }

        /// <inheritdoc/>
        public async Task<int> GetAvailableSeatsAsync(string flightNumber)
        {
            var flight = await _db.Flights
                .Include(f => f.Reservations)
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber);

            return flight?.AvailableSeats ?? 0;
        }
    }
}
