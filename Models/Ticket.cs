namespace FlightManagementSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string SeatNumber { get; set; }
        public DateTime IssueDate { get; set; }

        public Reservation Reservation { get; set; }
    }
}
