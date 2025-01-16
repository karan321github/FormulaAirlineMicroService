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
    }
}
