using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for passenger management (REQ-14 to REQ-18).
    /// </summary>
    public interface IPassengerService
    {
        Task<IEnumerable<Passenger>> GetAllAsync();
        Task<Passenger?> GetByIdAsync(int id);
        Task<Passenger?> GetByIdentityUserIdAsync(string identityUserId);
        Task<IEnumerable<Passenger>> SearchAsync(string query);
        Task CreateAsync(Passenger passenger);
        Task UpdateAsync(Passenger passenger);
        Task DeleteAsync(int id);
        Task<bool> ExistsForUserAsync(string identityUserId);
    }
}
