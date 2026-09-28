namespace formulaAirline.Api.Model
{
    public class User
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
        public string Role { get; set; } = "User"; // Default role is "User"
        public String Phone { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
