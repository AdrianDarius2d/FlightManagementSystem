namespace FlightManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public Passenger Passenger { get; set; }
        public Admin Admin { get; set; }
    }
}
