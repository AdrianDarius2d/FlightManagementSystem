using System.ComponentModel.DataAnnotations;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Stores in-app notifications delivered to users (REQ-46 to REQ-53).
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        /// <summary>IdentityUser.Id of the recipient.</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>Confirmation | Cancellation | FlightChange | Alert | System</summary>
        public string Type { get; set; } = "Alert";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
