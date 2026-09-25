# Circular Reference Fix Summary

## Issue
**Exception**: `System.Text.Json.JsonException: A possible object cycle was detected`

The circular reference occurred when serializing a `Booking` object because:
- `Booking.Flight` → Referenced `Flight` entity
- `Flight.Bookings` → Referenced collection of `Booking` entities (creating a cycle)
- The JSON serializer would traverse: `Booking.Flight.Bookings[0].Flight.Bookings[0].Flight...` until exceeding the max depth of 64

**Original Error Location**: `BookingController.CreatingBooking()` at line 89

## Solution Implemented

Created Data Transfer Objects (DTOs) to break the circular reference by excluding navigation collections from serialization.

### Files Created:

#### 1. `formulaAirline.Api/Model/BookingDto.cs`
```csharp
public class BookingDto
{
	public int Id { get; set; }
	public string PassangerName { get; set; }
	public string PassportNb { get; set; }
	public string From { get; set; }
	public string To { get; set; }
	public int Status { get; set; }
	public int FlightId { get; set; }
	public FlightDto? Flight { get; set; }  // Navigation property included
	// NOTE: Intentionally excluded Payments collection
}

public class FlightDto
{
	public int Id { get; set; }
	public string FlightNumber { get; set; }
	public string Departure { get; set; }
	public string Arrival { get; set; }
	public int Capacity { get; set; }
	public int AvailableSeats { get; set; }
	public DateTime DepartureTime { get; set; }
	public DateTime ArrivalTime { get; set; }
	public decimal Price { get; set; }
	// NOTE: Intentionally excluded Bookings collection to avoid circular reference
}
```

### Files Modified:

#### 2. `formulaAirline.Api/Controllers/BookingController.cs`
**Added**:
- `MapToDto(Booking booking)` helper method for entity-to-DTO conversion
- Converts `Booking` entity to `BookingDto` with simplified `FlightDto`

**Updated Methods**:
- `GetBooking()` - Returns `BookingDto` instead of `Booking`
- `GetAllBookings()` - Returns `IEnumerable<BookingDto>` instead of `Booking`
- `CreatingBooking()` - Returns `BookingDto` in the response body (fixes the original exception)
- `GetBookingsByPassenger()` - Returns `IEnumerable<BookingDto>` instead of `Booking`

#### 3. `formulaAirline.Api/Controllers/FlightController.cs`
**Added**:
- `MapToDto(Flight flight)` helper method for entity-to-DTO conversion
- Converts `Flight` entity to simplified `FlightDto` without the `Bookings` collection

**Updated Methods**:
- `GetFlight()` - Returns `FlightDto` instead of `Flight`
- `GetAllFlights()` - Returns `IEnumerable<FlightDto>` instead of `Flight`
- `SearchFlights()` - Returns `IEnumerable<FlightDto>` instead of `Flight`
- `CreateFlight()` - Returns `FlightDto` in the response body

## Why This Approach

1. **Breaks Circular References**: DTOs exclude collections that cause cycles
2. **API Isolation**: DTOs separate API contracts from internal entities
3. **Performance**: Clients receive only necessary data without circular references
4. **Maintainability**: Clear separation between entity and API layers

## Testing Recommendations

1. Test creating a booking via `POST /booking` - should no longer throw JsonException
2. Test retrieving bookings via `GET /booking/{id}` - should return clean DTO objects
3. Test all flight endpoints to verify DTOs are returned correctly
4. Verify the response structure matches expected API contracts

## Related Entities

If `Payment` model also has navigation back to `Booking`, consider creating a `PaymentDto` to prevent similar issues:
- Exclude the `Booking` navigation property from `PaymentDto` serialization
