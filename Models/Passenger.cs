using System.ComponentModel.DataAnnotations;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Stores the personal profile of a passenger, linked to their Identity account.
    /// </summary>
    public class Passenger
    {
        public int Id { get; set; }

        /// <summary>The IdentityUser.Id of the registered user who owns this profile.</summary>
        public string? IdentityUserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Passport / ID Number")]
        public string PassportId { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
