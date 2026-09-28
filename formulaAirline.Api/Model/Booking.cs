namespace formulaAirline.Api.Model
{
    public class Booking
    {
        public int Id { get; set; }
        public string PassangerName { get; set; } = "";
        public string PassportNb { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public int status { get; set; }

        // Foreign key for Flight
        public int FlightId { get; set; }
        public string UserId { get; set; } = ""; // Foreign key for User
        // Navigation properties
        public Flight? Flight { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
