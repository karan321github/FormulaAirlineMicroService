namespace formulaAirline.Api.Model
{
    public class BookingDto
    {
        public int Id { get; set; }
        public string PassangerName { get; set; } = "";
        public string PassportNb { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public int Status { get; set; }
        public int FlightId { get; set; }

        // Simplified payment info without circular reference
        public ICollection<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
    }

    public class FlightDto
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
        // NOTE: Intentionally excluded Bookings collection to avoid circular reference
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = "";
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        // NOTE: Intentionally excluded Booking navigation property to avoid circular reference
    }
}
