using formulaAirline.Api.Model;

namespace formulaAirline.Api.Services
{
    public interface IFlightService
    {
        Task<Flight?> GetFlightByIdAsync(int flightId);
        Task<IEnumerable<Flight>> GetAllFlightsAsync();
        Task<IEnumerable<Flight>> SearchFlightsAsync(string departure, string arrival);
        Task<Flight> CreateFlightAsync(Flight flight);
        Task UpdateFlightAsync(Flight flight);
        Task DeleteFlightAsync(int flightId);
        Task<bool> IsSeatsAvailableAsync(int flightId, int seatsRequired);
        Task ReserveSeatsAsync(int flightId, int seatsCount);
    }
}
