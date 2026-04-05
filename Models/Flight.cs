namespace FlightManagementSystem.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Flight
    {
        [Key]
        public string FlightNumber { get; set; }
        public int Capacity { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
    }
}
