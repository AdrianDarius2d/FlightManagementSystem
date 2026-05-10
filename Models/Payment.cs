using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Records payment information associated with a reservation.
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string Method { get; set; } = "Credit Card";

        [Display(Name = "Transaction Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Completed";

        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString()[..8].ToUpper();

        // Navigation
        public Reservation Reservation { get; set; } = null!;
    }
}
