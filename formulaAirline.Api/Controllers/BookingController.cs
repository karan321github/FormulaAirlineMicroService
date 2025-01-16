using formulaAirline.Api.Model;
using formulaAirline.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace formulaAirline.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class BookingController : Controller
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IMessageProducer _messageProducer;
        public static List<Booking> bookings = new();

        public BookingController(ILogger<BookingController>logger , IMessageProducer messageProducer)
        {
            _logger = logger;
            _messageProducer = messageProducer; 
        }
        [HttpPost]
        public IActionResult CreatingBooking(Booking booking)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bookings.Add(booking);
            _messageProducer.SendingMessages<Booking>(booking);

            return Ok("Booking created successfully");
        }
    }
}
