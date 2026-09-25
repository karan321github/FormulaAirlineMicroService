using AutoMapper;
using formulaAirline.Api.Model;

namespace formulaAirline.Api.Mappings
{
    /// <summary>
    /// AutoMapper configuration for mapping between entities and DTOs
    /// Prevents circular reference issues during JSON serialization
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Booking to BookingDto mapping
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ReverseMap()
                .ForMember(dest => dest.status, opt => opt.MapFrom(src => src.Status));

            // Flight to FlightDto mapping
            // Intentionally exclude Bookings collection to prevent circular reference
            CreateMap<Flight, FlightDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightNumber))
                .ForMember(dest => dest.Departure, opt => opt.MapFrom(src => src.Departure))
                .ForMember(dest => dest.Arrival, opt => opt.MapFrom(src => src.Arrival))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
                .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.AvailableSeats))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.DepartureTime))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => src.ArrivalTime))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));
                // NOTE: Bookings collection intentionally not mapped

            // Payment to PaymentDto mapping
            // Intentionally exclude Booking navigation property to prevent circular reference
            CreateMap<Payment, PaymentDto>()
                .ReverseMap();
                // NOTE: Booking navigation property intentionally not mapped
        }
    }
}
