namespace FlightManagementSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public float Amount { get; set; }
        public string Method { get; set; }
        public DateTime Date { get; set; }

        public Reservation Reservation { get; set; }
    }
}
