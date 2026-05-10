using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for payment processing (REQ-38 to REQ-45).
    /// </summary>
    public interface IPaymentService
    {
        Task<Payment> ProcessAsync(int reservationId, string method, decimal amount);
        Task<Payment?> GetByReservationIdAsync(int reservationId);
        Task<IEnumerable<Payment>> GetAllAsync();
    }
}
