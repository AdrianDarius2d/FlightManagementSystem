namespace FlightManagementSystem.Models
{
    
        public class Passenger
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int PassportId { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }

            public User User { get; set; }
            public ICollection<Reservation> Reservations { get; set; }
        }
    
}
