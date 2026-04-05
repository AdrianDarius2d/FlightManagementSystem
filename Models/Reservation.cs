using System.Net.Sockets;

namespace FlightManagementSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int PassengerId { get; set; }
        public string FlightId { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string ClassType { get; set; }

        public Passenger Passenger { get; set; }
        public Flight Flight { get; set; }
        public Ticket Ticket { get; set; }
        public Payment Payment { get; set; }
    }
}
