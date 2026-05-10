using System.ComponentModel.DataAnnotations;

namespace FlightManagementSystem.Models
{
    /// <summary>
    /// Stores system error events and exceptions.
    /// Satisfies REQ-68 (record system errors) and REQ-69 (review error logs).
    /// Also satisfies SRS §5.4.25 (logging mechanisms for errors).
    /// </summary>
    public class ErrorLog
    {
        /// <summary>Primary key.</summary>
        public int Id { get; set; }

        /// <summary>Error severity: Error, Warning, Critical.</summary>
        [Required]
        [Display(Name = "Severity")]
        public string Severity { get; set; } = "Error";

        /// <summary>Short error message.</summary>
        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>Full stack trace if available.</summary>
        [Display(Name = "Stack Trace")]
        public string? StackTrace { get; set; }

        /// <summary>URL / route that caused the error.</summary>
        [Display(Name = "Request Path")]
        public string? RequestPath { get; set; }

        /// <summary>HTTP method (GET/POST etc.) if applicable.</summary>
        [Display(Name = "HTTP Method")]
        public string? HttpMethod { get; set; }

        /// <summary>IdentityUser.Id of the logged-in user when the error occurred.</summary>
        [Display(Name = "User ID")]
        public string? UserId { get; set; }

        /// <summary>IP address of the request.</summary>
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        /// <summary>When the error occurred.</summary>
        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>Whether an administrator has acknowledged this error.</summary>
        [Display(Name = "Acknowledged")]
        public bool IsAcknowledged { get; set; } = false;
    }
}
