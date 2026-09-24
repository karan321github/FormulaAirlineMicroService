using formulaAirline.Api.Model;
using formulaAirline.Api.Repository;

namespace formulaAirline.Api.Services
{
    public class FlightService : IFlightService
    {
        private readonly IRepository<Flight> _flightRepository;
        private readonly ILogger<FlightService> _logger;

        public FlightService(IRepository<Flight> flightRepository, ILogger<FlightService> logger)
        {
            _flightRepository = flightRepository;
            _logger = logger;
        }

        public async Task<Flight?> GetFlightByIdAsync(int flightId)
        {
            try
            {
                return await _flightRepository.GetByIdAsync(flightId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving flight with ID {flightId}: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            try
            {
                return await _flightRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all flights: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string departure, string arrival)
        {
            try
            {
                return await _flightRepository.FindAsync(f => 
                    f.Departure.ToLower() == departure.ToLower() && 
                    f.Arrival.ToLower() == arrival.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching flights from {departure} to {arrival}: {ex.Message}");
                throw;
            }
        }

        public async Task<Flight> CreateFlightAsync(Flight flight)
        {
            try
            {
                flight.AvailableSeats = flight.Capacity;
                await _flightRepository.AddAsync(flight);
                _logger.LogInformation($"Flight {flight.FlightNumber} created successfully");
                return flight;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating flight: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateFlightAsync(Flight flight)
        {
            try
            {
                await _flightRepository.UpdateAsync(flight);
                _logger.LogInformation($"Flight {flight.Id} updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating flight: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteFlightAsync(int flightId)
        {
            try
            {
                var flight = await _flightRepository.GetByIdAsync(flightId);
                if (flight != null)
                {
                    await _flightRepository.DeleteAsync(flight);
                    _logger.LogInformation($"Flight {flightId} deleted successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting flight: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> IsSeatsAvailableAsync(int flightId, int seatsRequired)
        {
            try
            {
                var flight = await _flightRepository.GetByIdAsync(flightId);
                return flight != null && flight.AvailableSeats >= seatsRequired;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking seat availability: {ex.Message}");
                throw;
            }
        }

        public async Task ReserveSeatsAsync(int flightId, int seatsCount)
        {
            try
            {
                var flight = await _flightRepository.GetByIdAsync(flightId);
                if (flight != null && flight.AvailableSeats >= seatsCount)
                {
                    flight.AvailableSeats -= seatsCount;
                    await _flightRepository.UpdateAsync(flight);
                    _logger.LogInformation($"{seatsCount} seats reserved on flight {flightId}");
                }
                else
                {
                    throw new InvalidOperationException($"Not enough available seats on flight {flightId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reserving seats: {ex.Message}");
                throw;
            }
        }
    }
}
