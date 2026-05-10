using FlightManagementSystem.Data;
using FlightManagementSystem.Models;
using FlightManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightManagementSystem.Services
{
    /// <summary>
    /// Implements passenger management business logic (REQ-14 to REQ-18).
    /// </summary>
    public class PassengerService : IPassengerService
    {
        private readonly ApplicationDbContext _db;

        public PassengerService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Passenger>> GetAllAsync()
        {
            return await _db.Passengers
                .Include(p => p.Reservations)
                .OrderBy(p => p.LastName)
                .ToListAsync();
        }

        public async Task<Passenger?> GetByIdAsync(int id)
        {
            return await _db.Passengers
                .Include(p => p.Reservations)
                    .ThenInclude(r => r.Flight)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Passenger?> GetByIdentityUserIdAsync(string identityUserId)
        {
            return await _db.Passengers
                .FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId);
        }

        public async Task<IEnumerable<Passenger>> SearchAsync(string query)
        {
            // REQ-17: search by name or identification data
            return await _db.Passengers
                .Where(p =>
                    p.FirstName.Contains(query) ||
                    p.LastName.Contains(query) ||
                    p.Email.Contains(query) ||
                    p.PassportId.Contains(query))
                .ToListAsync();
        }

        public async Task CreateAsync(Passenger passenger)
        {
            _db.Passengers.Add(passenger);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Passenger passenger)
        {
            _db.Passengers.Update(passenger);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var passenger = await _db.Passengers.FindAsync(id);
            if (passenger != null)
            {
                _db.Passengers.Remove(passenger);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsForUserAsync(string identityUserId)
        {
            return await _db.Passengers.AnyAsync(p => p.IdentityUserId == identityUserId);
        }
    }
}
