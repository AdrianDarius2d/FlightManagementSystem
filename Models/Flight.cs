using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Represents a scheduled airline flight.
    /// </summary>
    public class Flight
    {
        [Key]
        [Display(Name = "Flight Number")]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "From (City / Airport)")]
        public string Source { get; set; } = string.Empty;

        [Required]
        [Display(Name = "To (City / Airport)")]
        public string Destination { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Departure")]
        public DateTime DepartureTime { get; set; }

        [Required]
        [Display(Name = "Arrival")]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Total Capacity")]
        public int Capacity { get; set; }

        [Display(Name = "Aircraft Type")]
        public string AircraftType { get; set; } = "Boeing 737";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 100000)]
        [Display(Name = "Base Price (USD)")]
        public decimal Price { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Scheduled";

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        [NotMapped]
        public int BookedSeats => Reservations?.Count(r => r.Status != "Cancelled") ?? 0;

        [NotMapped]
        public int AvailableSeats => Capacity - BookedSeats;
    }
}
