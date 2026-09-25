# ⚠️ Common Pitfalls & Best Practices
## Lessons from Production Microservices

---

## 🚫 10 Critical Mistakes to Avoid

### 1. **Too Many Services Too Soon (Micro-Service Hell)**

❌ **Wrong Approach**:
```
Version 1: Created 15 microservices immediately
Result: Complexity explosion, testing nightmare, DevOps overhead
Time to market: 6 months (should be 2 weeks)
```

✅ **Correct Approach**:
```
Start with 2-3 logical services:
- Flight Service (reads mostly)
- Booking Service (writes, orchestrates)
- Payment Service (isolated concern)

Then expand based on team size and performance needs
```

**Rule**: Start with monolith, refactor to microservices when you have:
- Team size > 5 developers
- Performance bottleneck identified
- Clear service boundaries identified

---

### 2. **Chatty Services (N+1 Call Problem)**

❌ **Bad Example**:
```csharp
public async Task<BookingDto> GetBookingDetails(int bookingId)
{
	var booking = await _bookingService.GetBookingAsync(bookingId);

	// 🔴 Makes an HTTP call for every field!
	var flight = await _flightService.GetFlightAsync(booking.FlightId);
	var passenger = await _passengerService.GetPassengerAsync(booking.PassengerId);
	var payment = await _paymentService.GetPaymentAsync(booking.PaymentId);

	// For 100 bookings, this is 400 calls instead of 100!
}
```

✅ **Good Solution**:
```csharp
// Option 1: Cache frequently accessed data
public async Task<BookingDto> GetBookingDetails(int bookingId)
{
	var booking = await _bookingService.GetBookingAsync(bookingId);

	// Parallel requests (4 calls happens in parallel, not sequential)
	var tasks = await Task.WhenAll(
		_flightService.GetFlightAsync(booking.FlightId),
		_passengerService.GetPassengerAsync(booking.PassengerId),
		_paymentService.GetPaymentAsync(booking.PaymentId)
	);

	return MapToDto(booking, tasks);
}

// Option 2: Batch query
public async Task<List<BookingDto>> GetBookingsList(List<int> bookingIds)
{
	// Ask service to get data for multiple IDs at once
	var bookings = await _bookingService.GetBookingsAsync(bookingIds); // 1 call
	var flights = await _flightService.GetFlightsBatchAsync(
		bookings.Select(b => b.FlightId).Distinct() // 1 call
	);

	return MapToDto(bookings, flights);
}
```

---

### 3. **Tight Coupling Between Services**

❌ **Bad Example**:
```csharp
// Booking Service directly depends on Flight Service implementation
public class BookingService
{
	private readonly FlightService _flightService; // ❌ Direct dependency

	public async Task CreateBookingAsync(CreateBookingRequest request)
	{
		var flight = await _flightService.GetFlightAsync(request.FlightId);
		// If Flight Service changes, Booking Service breaks!
	}
}
```

✅ **Good Solution**:
```csharp
// Use events/contracts instead
public class BookingService
{
	private readonly IHttpClientFactory _httpClientFactory; // ✅ Loosely coupled

	public async Task CreateBookingAsync(CreateBookingRequest request)
	{
		var client = _httpClientFactory.CreateClient("FlightService");
		var response = await client.GetAsync($"/api/flights/{request.FlightId}");

		// Works regardless of Flight Service's internal implementation
	}
}

// Better: Use events so services don't call each other directly
public async Task CreateBookingAsync(CreateBookingRequest request)
{
	var booking = new Booking(request);
	await _repository.AddAsync(booking);

	// ✅ Publish event, Flight Service will handle independently
	await _eventPublisher.PublishAsync(new BookingCreatedEvent(booking));
}
```

---

### 4. **Shared Database Between Services**

❌ **Wrong Architecture**:
```
Booking Service ──┐
				  ├──→ Shared Database
Flight Service ──┘
Payment Service ─┘

Problems:
- Can't scale Flight Service independently
- Database migration requires coordination
- Services are tightly coupled at DB level
- One service's query can lock data for others
```

✅ **Correct Architecture**:
```
Booking Service → Booking DB
Flight Service → Flight DB
Payment Service → Payment DB

Services communicate via:
- REST APIs
- Message Bus (Events)
- gRPC (for high-performance calls)
```

---

### 5. **Synchronous Communication Everywhere**

❌ **Bad Pattern**:
```csharp
// Booking waits for Payment to complete
public async Task CreateBookingAsync(CreateBookingRequest request)
{
	var booking = new Booking(request);

	// 🔴 Blocking call - if Payment Service is slow, Booking is slow
	var paymentResult = await _paymentService.ProcessPaymentAsync(booking);

	if (paymentResult.Success)
		await _repository.AddAsync(booking);
}
```

✅ **Better Pattern (Async/Event-Driven)**:
```csharp
// Booking completes immediately, Payment happens asynchronously
public async Task CreateBookingAsync(CreateBookingRequest request)
{
	var booking = new Booking(request);
	booking.Status = BookingStatus.Pending; // Not confirmed yet

	await _repository.AddAsync(booking);

	// ✅ Fire-and-forget: Publish event, Booking Service returns immediately
	await _eventPublisher.PublishAsync(new BookingCreatedEvent(booking));

	// Payment Service processes event asynchronously
	// Booking gets updated via another event when payment completes
}
```

---

### 6. **Not Handling Service Failures**

❌ **No Resilience**:
```csharp
public async Task<Flight> GetFlightAsync(int id)
{
	// 🔴 If Flight Service dies, all requests fail immediately
	var response = await _httpClient.GetAsync($"/flights/{id}");
	return JsonConvert.DeserializeObject<Flight>(await response.Content.ReadAsStringAsync());
}
```

✅ **With Resilience**:
```csharp
public async Task<Flight> GetFlightAsync(int id)
{
	var policy = Policy
		.Handle<HttpRequestException>()
		.OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
		.FallbackAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
		{
			Content = new StringContent(JsonConvert.SerializeObject(
				new Flight { Id = id, CachedData = true })) // Fallback to cache
		})
		.WrapAsync(
			Policy
				.Handle<HttpRequestException>()
				.WaitAndRetryAsync(new[] {
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(2),
					TimeSpan.FromSeconds(4)
				}) // Retry with exponential backoff
		);

	var response = await policy.ExecuteAsync(
		() => _httpClient.GetAsync($"/flights/{id}")
	);

	return JsonConvert.DeserializeObject<Flight>(
		await response.Content.ReadAsStringAsync()
	);
}
```

---

### 7. **Not Idempotent Message Handlers**

❌ **Problem**:
```csharp
public async Task HandleBookingCreatedAsync(BookingCreatedEvent @event)
{
	// 🔴 If message is processed twice, payment is created twice!
	var payment = new Payment { BookingId = @event.BookingId };
	await _repository.AddAsync(payment);
}

// Result: Duplicate payments if message is retried
```

✅ **Idempotent Handler**:
```csharp
public async Task HandleBookingCreatedAsync(BookingCreatedEvent @event)
{
	// Check if already processed using event ID
	var existingPayment = await _repository.FindAsync(
		p => p.ExternalEventId == @event.Id
	);

	if (existingPayment != null)
	{
		_logger.LogInformation("Booking already processed: {EventId}", @event.Id);
		return; // ✅ Safe to process multiple times
	}

	var payment = new Payment 
	{ 
		BookingId = @event.BookingId,
		ExternalEventId = @event.Id // Track the external event
	};
	await _repository.AddAsync(payment);
}
```

---

### 8. **Ignoring Data Consistency Issues**

❌ **Race Condition**:
```csharp
// Thread 1: Book seat
var flight = await _flightService.GetFlightAsync(1); // AvailableSeats = 1
await _flightService.ReserveSeatsAsync(1, 1);

// Thread 2: Book same seat (at the same time)
var flight = await _flightService.GetFlightAsync(1); // AvailableSeats = 1
await _flightService.ReserveSeatsAsync(1, 1); // Overbooking!
```

✅ **Pessimistic Locking**:
```csharp
public async Task<bool> ReserveSeatsAsync(int flightId, int quantity)
{
	using var transaction = await _context.Database.BeginTransactionAsync();

	var flight = await _context.Flights
		.FromSqlRaw("SELECT * FROM Flights WHERE Id = {0} WITH (UPDLOCK)", flightId)
		.FirstOrDefaultAsync(); // ✅ Locked during transaction

	if (flight.AvailableSeats < quantity)
	{
		await transaction.RollbackAsync();
		return false;
	}

	flight.AvailableSeats -= quantity;
	await _context.SaveChangesAsync();
	await transaction.CommitAsync();

	return true;
}
```

---

### 9. **Missing Distributed Tracing**

❌ **No Visibility**:
```
User Request
├→ Booking Service ✅ (200ms)
├→ Flight Service ❌ (Timeout after 30s)
├→ Payment Service ? (Unknown)
└→ Response: 500 Internal Server Error

Question: Which service failed? Why? How do I debug?
Answer: No idea! No tracing!
```

✅ **With Correlation IDs**:
```csharp
public class CorrelationIdMiddleware
{
	public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
	{
		var correlationId = context.Request.Headers.ContainsKey("X-Correlation-ID")
			? context.Request.Headers["X-Correlation-ID"].ToString()
			: Guid.NewGuid().ToString();

		context.Items["CorrelationId"] = correlationId;

		using (LogContext.PushProperty("CorrelationId", correlationId))
		{
			await _next(context);
		}
	}
}

// Usage in services
_logger.LogInformation("Processing booking: {BookingId}", bookingId);
// Output: "Processing booking: 123 | CorrelationId: abc-def-123"

// Pass to other services
var request = new HttpRequestMessage(...);
request.Headers.Add("X-Correlation-ID", correlationId);
await _httpClient.SendAsync(request);

// RabbitMQ messages
var properties = channel.CreateBasicProperties();
properties.Headers = new Dictionary<string, object> { ["X-Correlation-ID"] = correlationId };
```

---

### 10. **Not Planning for Monitoring & Alerting**

❌ **Flying Blind**:
```
- No metrics collection
- No health checks
- Discovered issues via user complaints
- Can't compare performance over time
- No capacity planning data
```

✅ **Proper Monitoring**:
```csharp
public class BookingMetrics
{
	private readonly IMetricsCollector _metrics;

	public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
	{
		var stopwatch = Stopwatch.StartNew();

		try
		{
			var booking = new Booking(request);
			await _repository.AddAsync(booking);

			stopwatch.Stop();
			_metrics.RecordHistogram("booking_creation_duration_ms", stopwatch.ElapsedMilliseconds);
			_metrics.IncrementCounter("bookings_created_total");

			return _mapper.Map<BookingDto>(booking);
		}
		catch (Exception ex)
		{
			_metrics.IncrementCounter("booking_creation_errors_total");
			throw;
		}
	}
}

// Health Check Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
	Predicate = _ => true,
	ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

## ✅ 10 Best Practices to Follow

### 1. **API Versioning**
```csharp
// Support multiple API versions gracefully
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookingsController : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> CreateBooking_V1(CreateBookingRequestV1 request)
	{
		// Version 1 logic
	}
}

// New version without breaking existing clients
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BookingsV2Controller : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> CreateBooking_V2(CreateBookingRequestV2 request)
	{
		// Improved logic
	}
}
```

### 2. **Backward Compatibility**
```csharp
// Old clients still work with new version
public class BookingDto
{
	public int Id { get; set; }

	// Old properties (must exist for backward compatibility)
	[Obsolete("Use FullPassengerName instead")]
	public string PassengerName { get; set; }

	// New properties
	public string FullPassengerName { get; set; }

	// Handle both
	public string GetPassengerName() => 
		!string.IsNullOrEmpty(FullPassengerName) 
			? FullPassengerName 
			: PassengerName;
}
```

### 3. **Comprehensive Logging**
```csharp
_logger.LogInformation(
	"Booking created: {@Booking}. Flight: {@Flight}. Processing time: {Duration}ms",
	booking, flight, stopwatch.ElapsedMilliseconds);

_logger.LogWarning(
	"Booking creation took unexpectedly long: {Duration}ms (threshold: 5000ms)",
	stopwatch.ElapsedMilliseconds);

_logger.LogError(exception,
	"Error creating booking for passenger {PassengerId}: {ErrorMessage}",
	request.PassengerId, exception.Message);
```

### 4. **Implement Circuit Breaker**
```csharp
var circuitBreakerPolicy = Policy
	.Handle<HttpRequestException>()
	.OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
	.CircuitBreakerAsync(
		handledEventsAllowedBeforeBreaking: 5,  // Fail 5 times before breaking
		durationOfBreak: TimeSpan.FromSeconds(30), // Wait 30s before retry
		onBreak: (outcome, duration) =>
		{
			_logger.LogWarning(
				"Circuit breaker opened for FlightService. Waiting {Duration}s",
				duration.TotalSeconds);
		},
		onReset: () =>
		{
			_logger.LogInformation("Circuit breaker reset for FlightService");
		}
	);
```

### 5. **Validate at Service Boundary**
```csharp
// Use FluentValidation
public class CreateBookingValidator : AbstractValidator<CreateBookingRequest>
{
	public CreateBookingValidator()
	{
		RuleFor(x => x.FlightId).GreaterThan(0);
		RuleFor(x => x.PassengerId).GreaterThan(0);
		RuleFor(x => x.PassengerName)
			.NotEmpty()
			.Length(2, 100);
		RuleFor(x => x.Email).EmailAddress();
		RuleFor(x => x.PassportNumber)
			.Matches(@"^[A-Z0-9]{6,9}$"); // Example: ABC123456
	}
}

// Use in Controller
[HttpPost]
public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
{
	var validationResult = await _validator.ValidateAsync(request);
	if (!validationResult.IsValid)
		return BadRequest(validationResult.Errors);

	// Process valid request
}
```

### 6. **Implement Saga Pattern for Distributed Transactions**
```csharp
public class BookingCreationSaga
{
	public async Task ExecuteAsync(CreateBookingRequest request)
	{
		var booking = new Booking(request);
		var compensations = new List<Func<Task>>();

		try
		{
			// Step 1: Create booking
			await _bookingRepository.AddAsync(booking);
			compensations.Add(() => _bookingRepository.DeleteAsync(booking));

			// Step 2: Reserve seats
			await _flightService.ReserveSeatsAsync(booking.FlightId, 1);
			compensations.Add(() => _flightService.ReleaseSeatAsync(booking.FlightId, 1));

			// Step 3: Process payment
			var paymentResult = await _paymentService.ProcessPaymentAsync(booking);
			compensations.Add(() => _paymentService.RefundAsync(paymentResult.PaymentId));

			// All succeeded
			booking.Status = BookingStatus.Confirmed;
			await _bookingRepository.UpdateAsync(booking);
		}
		catch (Exception ex)
		{
			// Rollback in reverse order
			foreach (var compensation in ((IEnumerable<Func<Task>>)compensations).Reverse())
			{
				try { await compensation(); }
				catch { /* Log but continue */ }
			}
			throw;
		}
	}
}
```

### 7. **Use CQRS for Complex Operations**
```csharp
// Query (Read)
public class GetUserBookingsQuery : IRequest<IEnumerable<BookingDto>>
{
	public int UserId { get; set; }
}

// Command (Write)
public class CreateBookingCommand : IRequest<BookingDto>
{
	public CreateBookingRequest Request { get; set; }
}

// Benefits:
// - Separate read/write models
// - Optimize each independently
// - Scale read/write separately
// - Simpler to test and reason about
```

### 8. **Database Migration Strategy**
```csharp
// Use EF Core Migrations
dotnet ef migrations add AddBookingEntity
dotnet ef database update

// Always enable automatic migration on startup
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
	dbContext.Database.Migrate(); // ✅ Automatic and safe
}

// Zero-downtime migrations:
// 1. Add new column (nullable or with default)
// 2. Deploy services
// 3. Move data
// 4. Remove old column
```

### 9. **Error Response Standardization**
```csharp
public class ErrorResponse
{
	public string Code { get; set; }
	public string Message { get; set; }
	public string TraceId { get; set; }
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// All services return same format
// Makes frontend easier to handle
// Consistent error handling across platform
```

### 10. **Documentation Strategy**
```csharp
// Use Swagger/OpenAPI
public class BookingsController : ControllerBase
{
	/// <summary>
	/// Create a new flight booking
	/// </summary>
	/// <param name="request">Booking details</param>
	/// <returns>Created booking</returns>
	/// <response code="201">Booking created successfully</response>
	/// <response code="400">Invalid request</response>
	/// <response code="404">Flight not found</response>
	[HttpPost]
	[ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
	{
		// Implementation
	}
}

// Swagger will generate API docs automatically
// Accessible at /swagger/ui
```

---

## 🔍 Health Check Patterns

```csharp
public static IHealthChecksBuilder AddCustomHealthChecks(this IServiceCollection services)
{
	return services.AddHealthChecks()
		.AddCheck("database", () =>
		{
			try
			{
				using var connection = new SqlConnection(_connectionString);
				connection.Open();
				return HealthCheckResult.Healthy();
			}
			catch (Exception ex)
			{
				return HealthCheckResult.Unhealthy("Database connection failed", ex);
			}
		})
		.AddCheck("rabbitmq", () =>
		{
			try
			{
				using var connection = _rabbitConnection;
				using var channel = connection.CreateModel();
				channel.BasicQos(0, 1, false);
				return HealthCheckResult.Healthy();
			}
			catch (Exception ex)
			{
				return HealthCheckResult.Unhealthy("RabbitMQ connection failed", ex);
			}
		})
		.AddCheck("external_api", () =>
		{
			try
			{
				var response = _httpClient.GetAsync("https://api.example.com/health").Result;
				return response.IsSuccessStatusCode 
					? HealthCheckResult.Healthy() 
					: HealthCheckResult.Degraded("External API slow");
			}
			catch
			{
				return HealthCheckResult.Unhealthy("External API unreachable");
			}
		});
}
```

---

## 📊 Monitoring Checklist

- [ ] Response time metrics (p50, p95, p99)
- [ ] Error rate tracking
- [ ] Service availability tracking
- [ ] Database connection pool usage
- [ ] Message queue depth
- [ ] Memory usage per service
- [ ] CPU usage per service
- [ ] Disk space remaining
- [ ] Network bandwidth
- [ ] Active connections count

---

## 🎯 Summary

**Key Takeaways**:
1. ✅ Start simple, scale gradually
2. ✅ Decouple services via events
3. ✅ Each service = own database
4. ✅ Handle failures gracefully (Polly)
5. ✅ Make services idempotent
6. ✅ Trace requests across services
7. ✅ Monitor everything
8. ✅ Document as you go
9. ✅ Test at service boundaries
10. ✅ Plan for operations team

**Status**: ✅ Production-ready best practices guide complete
