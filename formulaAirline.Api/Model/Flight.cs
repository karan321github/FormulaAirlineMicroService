namespace formulaAirline.Api.Model
{
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = "";
        public string Departure { get; set; } = "";
        public string Arrival { get; set; } = "";
        public int Capacity { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }

        // Navigation property
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
