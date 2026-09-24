using formulaAirline.Api.Model;
using formulaAirline.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace formulaAirline.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightController> _logger;

        public FlightController(IFlightService flightService, ILogger<FlightController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlight(int id)
        {
            try
            {
                var flight = await _flightService.GetFlightByIdAsync(id);
                if (flight == null)
                    return NotFound($"Flight with ID {id} not found");

                return Ok(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving flight: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFlights()
        {
            try
            {
                var flights = await _flightService.GetAllFlightsAsync();
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving flights: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights([FromQuery] string departure, [FromQuery] string arrival)
        {
            try
            {
                if (string.IsNullOrEmpty(departure) || string.IsNullOrEmpty(arrival))
                    return BadRequest("Departure and arrival locations are required");

                var flights = await _flightService.SearchFlightsAsync(departure, arrival);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching flights: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFlight([FromBody] Flight flight)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdFlight = await _flightService.CreateFlightAsync(flight);
                return CreatedAtAction(nameof(GetFlight), new { id = createdFlight.Id }, createdFlight);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating flight: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] Flight flight)
        {
            try
            {
                if (id != flight.Id)
                    return BadRequest("ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _flightService.UpdateFlightAsync(flight);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating flight: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            try
            {
                await _flightService.DeleteFlightAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting flight: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/check-availability")]
        public async Task<IActionResult> CheckSeatAvailability(int id, [FromQuery] int seatsRequired)
        {
            try
            {
                var isAvailable = await _flightService.IsSeatsAvailableAsync(id, seatsRequired);
                return Ok(new { flightId = id, seatsRequired, isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking seat availability: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
