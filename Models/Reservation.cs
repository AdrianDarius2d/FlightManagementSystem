using System.ComponentModel.DataAnnotations;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Represents a passenger's booking for a specific flight.
    /// </summary>
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int PassengerId { get; set; }

        [Required]
        [Display(Name = "Flight")]
        public string FlightId { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Confirmed";

        [Display(Name = "Booking Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Class")]
        public string ClassType { get; set; } = "Economy";

        // Navigation
        public Passenger Passenger { get; set; } = null!;
        public Flight Flight { get; set; } = null!;
        public Ticket? Ticket { get; set; }
        public Payment? Payment { get; set; }
    }
}
