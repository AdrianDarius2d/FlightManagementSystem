using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for in-app notification delivery (REQ-46 to REQ-53).
    /// </summary>
    public interface INotificationService
    {
        Task SendAsync(string userId, string message, string type = "Alert");
        Task<IEnumerable<Notification>> GetForUserAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
