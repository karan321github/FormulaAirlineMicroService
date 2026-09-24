namespace formulaAirline.Api.Model
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = "";
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }

        // Navigation property
        public Booking? Booking { get; set; }
    }
}
