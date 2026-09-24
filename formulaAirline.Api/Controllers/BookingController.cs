using formulaAirline.Api.Model;
using formulaAirline.Api.Repository;
using formulaAirline.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace formulaAirline.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IMessageProducer _messageProducer;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IFlightService _flightService;

        public BookingController(
            ILogger<BookingController> logger,
            IMessageProducer messageProducer,
            IRepository<Booking> bookingRepository,
            IFlightService flightService)
        {
            _logger = logger;
            _messageProducer = messageProducer;
            _bookingRepository = bookingRepository;
            _flightService = flightService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                    return NotFound($"Booking with ID {id} not found");

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving booking: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingRepository.GetAllAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving bookings: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatingBooking([FromBody] Booking booking)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validate flight exists and has available seats
                var flight = await _flightService.GetFlightByIdAsync(booking.FlightId);
                if (flight == null)
                    return BadRequest($"Flight with ID {booking.FlightId} not found");

                if (!await _flightService.IsSeatsAvailableAsync(booking.FlightId, 1))
                    return BadRequest("No available seats on this flight");

                // Reserve seat
                await _flightService.ReserveSeatsAsync(booking.FlightId, 1);

                // Create booking
                booking.status = 1; // Confirmed status
                await _bookingRepository.AddAsync(booking);

                // Send message through message producer
                _messageProducer.SendingMessages<Booking>(booking);

                _logger.LogInformation($"Booking created successfully for passenger {booking.PassangerName}");
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating booking: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] Booking booking)
        {
            try
            {
                if (id != booking.Id)
                    return BadRequest("ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingBooking = await _bookingRepository.GetByIdAsync(id);
                if (existingBooking == null)
                    return NotFound($"Booking with ID {id} not found");

                // Update booking details
                existingBooking.PassangerName = booking.PassangerName;
                existingBooking.PassportNb = booking.PassportNb;
                existingBooking.From = booking.From;
                existingBooking.To = booking.To;
                existingBooking.status = booking.status;

                await _bookingRepository.UpdateAsync(existingBooking);
                _logger.LogInformation($"Booking {id} updated successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating booking: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                    return NotFound($"Booking with ID {id} not found");

                await _bookingRepository.DeleteAsync(booking);
                _logger.LogInformation($"Booking {id} deleted successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting booking: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("passenger/{passengerName}")]
        public async Task<IActionResult> GetBookingsByPassenger(string passengerName)
        {
            try
            {
                var bookings = await _bookingRepository.FindAsync(b => b.PassangerName.ToLower() == passengerName.ToLower());
                if (!bookings.Any())
                    return NotFound($"No bookings found for passenger {passengerName}");

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving bookings by passenger: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
