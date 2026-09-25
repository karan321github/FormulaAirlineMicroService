# 🏗️ Comprehensive Microservice Architecture & Scalability Guide
## FormulaAirline - From Monolith to Enterprise-Grade Platform

---

## 📊 Current State vs Target State

### Current Architecture (Monolith)
```
FormulaAirline.Api
├── Controllers (Booking, Flight, Payment, Weather)
├── Services (Flight Service, Payment Service, Message Producer)
├── Models (Booking, Flight, Payment)
├── Repository (Generic)
└── Database (Single SQL Server)
```

**Issues:**
- ❌ Payment logic tightly coupled with Booking
- ❌ Single point of failure
- ❌ Cannot scale payment service independently
- ❌ Difficult to maintain as features grow
- ❌ Limited by single database

### Target Architecture (Microservices)
```
API Gateway (Ocelot)
├── FormulaAirline.Gateway
│
├── FormulaAirline.Flight.Service
│   ├── Controllers
│   ├── Services
│   ├── Models
│   └── Database (Flight DB)
│
├── FormulaAirline.Payment.Service
│   ├── Controllers
│   ├── Services
│   ├── Models
│   └── Database (Payment DB)
│
├── FormulaAirline.Booking.Service
│   ├── Controllers
│   ├── Services
│   ├── Models
│   └── Database (Booking DB)
│
├── FormulaAirline.Notification.Service
│   ├── Services
│   └── Models
│
├── Message Bus (RabbitMQ/Azure Service Bus)
│   ├── Events.Booking (Published)
│   ├── Events.Payment (Published)
│   └── Events.Flight (Published)
│
└── Infrastructure
	├── EventBus
	├── Logging (Serilog)
	├── Caching (Redis)
	└── Service Discovery

```

**Advantages:**
- ✅ Independent deployment
- ✅ Technology flexibility per service
- ✅ Horizontal scaling
- ✅ Team autonomy
- ✅ Fault isolation

---

## 🛣️ Phase-by-Phase Implementation Roadmap

### Phase 1: Foundation & Infrastructure (Week 1-2)
- [ ] **API Gateway Setup** (Ocelot)
- [ ] **Message Bus Setup** (RabbitMQ)
- [ ] **Service Discovery** (Consul/Built-in)
- [ ] **Async Communication Patterns**
- [ ] **Logging & Monitoring** (Serilog, Seq)

### Phase 2: Service Decomposition (Week 3-4)
- [ ] **Flight Service** (Standalone)
- [ ] **Booking Service** (New repository pattern)
- [ ] **Payment Service** (As separate microservice)
- [ ] **Notification Service** (Email/SMS)

### Phase 3: Asynchronous Communication (Week 5-6)
- [ ] **Event Publishing** (Booking Created)
- [ ] **Event Subscribing** (Payment updates)
- [ ] **Dead Letter Queues**
- [ ] **Retry Policies**

### Phase 4: Data Management (Week 7-8)
- [ ] **Database per Service** (Database segregation)
- [ ] **Distributed Transactions** (Saga pattern)
- [ ] **Data Consistency**
- [ ] **Backup & Recovery**

### Phase 5: Testing & Resilience (Week 9-10)
- [ ] **Integration Tests**
- [ ] **Circuit Breaker Pattern** (Polly)
- [ ] **Resilience Testing**
- [ ] **Load Testing**

### Phase 6: Deployment & Monitoring (Week 11-12)
- [ ] **Docker Containerization**
- [ ] **Kubernetes Orchestration**
- [ ] **CI/CD Pipeline** (Azure DevOps/GitHub Actions)
- [ ] **Health Checks & Metrics**

---

## 🎯 Detailed Action Plan

### 1. API Gateway Implementation (Ocelot)

**Why Ocelot?**
- Lightweight, easy to configure
- Built for microservices
- Load balancing, rate limiting
- Perfect for learning

**Install NuGet Package:**
```bash
dotnet add package Ocelot
dotnet add package Ocelot.Provider.Consul
```

**Structure:**
```
FormulaAirline.Gateway/
├── Program.cs
├── ocelot.json (routing config)
└── Extensions/
	└── OcelotExtensions.cs
```

**Benefits:**
```
Client Requests
	↓
API Gateway (Single Entry Point)
	↓ (Routes to)
├── Flight Service (Port 5001)
├── Booking Service (Port 5002)
├── Payment Service (Port 5003)
└── Notification Service (Port 5004)
```

### 2. Message Bus Setup (RabbitMQ)

**Architecture:**
```
Publish-Subscribe Model

Event Publisher (Booking Service)
	↓
RabbitMQ Message Bus
	├→ Exchange: BookingEvents
	│   └→ Queue: BookingCreated
	│       └→ Payment Service (Subscriber)
	│       └→ Notification Service (Subscriber)
	│
	├→ Exchange: PaymentEvents
	│   └→ Queue: PaymentProcessed
	│       └→ Booking Service (Subscriber)
	│
	└→ Exchange: FlightEvents
		└→ Queue: SeatsReserved
			└→ Notification Service (Subscriber)
```

**Install Packages:**
```bash
dotnet add package RabbitMQ.Client
dotnet add package MassTransit
dotnet add package MassTransit.RabbitMQ
```

### 3. Event-Driven Communication

**Example Flow:**

```
1. User creates booking
   ↓ (POST /booking)
2. Booking Service
   ├→ Validates booking
   ├→ Reserves flight seats
   ├→ Saves to database
   ├→ Publishes "BookingCreated" event
   │   ├→ { BookingId, PassengerId, FlightId, Amount }
   │   └→ Timestamp: 2024-01-20T10:30:00Z
   ↓
3. Payment Service (Subscriber)
   ├→ Receives BookingCreated event
   ├→ Creates payment record
   ├→ Processes payment
   ├→ Publishes "PaymentProcessed" event
   │   ├→ { BookingId, Status, Amount }
   └→ Timestamp: 2024-01-20T10:30:15Z
   ↓
4. Booking Service (Subscriber)
   ├→ Receives PaymentProcessed event
   ├→ Updates booking status to "Confirmed"
   ├→ Publishes "BookingConfirmed" event
   ↓
5. Notification Service (Subscriber)
   ├→ Receives BookingConfirmed event
   ├→ Sends email/SMS to passenger
   └→ Updates notification log
```

### 4. Service Decomposition

#### Structure Each Service As:

```
FormulaAirline.{ServiceName}.Service/
├── src/
│   ├── FormulaAirline.{ServiceName}.API/
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── FormulaAirline.{ServiceName}.Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   │
│   ├── FormulaAirline.{ServiceName}.Application/
│   │   ├── DTOs/
│   │   ├── Services/
│   │   ├── Mappings/
│   │   └── Events/
│   │
│   ├── FormulaAirline.{ServiceName}.Infrastructure/
│   │   ├── Data/
│   │   │   ├── DbContext.cs
│   │   │   └── Migrations/
│   │   ├── ExternalServices/
│   │   ├── MessageBus/
│   │   └── Repositories/
│   │
│   └── FormulaAirline.{ServiceName}.Tests/
│       ├── UnitTests/
│       ├── IntegrationTests/
│       └── Fixtures/
│
└── docker-compose.yml
```

#### Current Services to Create:

**1. Flight Service** 
- Manages flights, seats, availability
- Event: `FlightSeatsReserved`

**2. Booking Service**
- Core booking logic
- Orchestrates other services
- Events: `BookingCreated`, `BookingConfirmed`, `BookingCanceled`

**3. Payment Service**
- Handles payment processing (can be free for now)
- Integration point for payment gateway later
- Events: `PaymentCreated`, `PaymentProcessed`, `PaymentFailed`

**4. Notification Service**
- Email, SMS, Push notifications
- Subscribes to booking/payment events
- No database needed (stateless)

**5. User Service** (New)
- Manages passenger profiles
- User registration, authentication
- JWT token management

---

## 🏆 Best Practices for Scalable .NET Projects

### 1. **SOLID Principles**

```csharp
// ❌ BAD: Violates Single Responsibility & Open/Closed
public class BookingService
{
	public void CreateBooking(Booking booking)
	{
		// Validate
		ValidateBooking(booking);

		// Save
		_db.Bookings.Add(booking);

		// Send email
		SendEmail();

		// Process payment
		ProcessPayment();

		// Update inventory
		UpdateInventory();
	}
}

// ✅ GOOD: Each class has single responsibility
public interface IBookingService 
{
	Task<BookingDto> CreateBookingAsync(CreateBookingRequest request);
}

public class BookingService : IBookingService
{
	private readonly IRepository<Booking> _bookingRepository;
	private readonly IBookingValidator _validator;
	private readonly IEventPublisher _eventPublisher;

	public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
	{
		// Validate
		var validationResult = await _validator.ValidateAsync(request);
		if (!validationResult.IsValid)
			throw new BookingValidationException(validationResult.Errors);

		// Create
		var booking = new Booking(request.FlightId, request.PassengerId);

		// Save
		await _bookingRepository.AddAsync(booking);

		// Publish event (other services handle email, payment, inventory)
		await _eventPublisher.PublishAsync(new BookingCreatedEvent(booking));

		return _mapper.Map<BookingDto>(booking);
	}
}
```

### 2. **Dependency Injection & IoC Container**

```csharp
// Program.cs - Configure all dependencies
builder.Services
	.AddScoped<IBookingService, BookingService>()
	.AddScoped<IFlightService, FlightService>()
	.AddScoped<IPaymentService, PaymentService>()
	.AddScoped(typeof(IRepository<>), typeof(Repository<>))
	.AddSingleton<IEventPublisher, RabbitMqEventPublisher>()
	.AddAutoMapper(typeof(MappingProfile))
	.AddMediatR(typeof(Program)) // For command/query pattern
	.AddLogging();
```

### 3. **Repository Pattern**

```csharp
public interface IRepository<T> where T : Entity
{
	Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);
	Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
	Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
	Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
	Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}

public class GenericRepository<T> : IRepository<T> where T : Entity
{
	private readonly DbContext _context;
	private readonly DbSet<T> _dbSet;

	public GenericRepository(DbContext context)
	{
		_context = context;
		_dbSet = context.Set<T>();
	}

	public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
	}

	// Implement other methods...
}
```

### 4. **Unit of Work Pattern**

```csharp
public interface IUnitOfWork : IDisposable
{
	IRepository<Booking> Bookings { get; }
	IRepository<Flight> Flights { get; }
	IRepository<Payment> Payments { get; }
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default);
	Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default);
	Task<bool> RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _context;
	private IRepository<Booking> _bookingRepository;
	private IRepository<Flight> _flightRepository;
	private IRepository<Payment> _paymentRepository;

	public IRepository<Booking> Bookings => 
		_bookingRepository ??= new Repository<Booking>(_context);

	public IRepository<Flight> Flights => 
		_flightRepository ??= new Repository<Flight>(_context);

	public IRepository<Payment> Payments => 
		_paymentRepository ??= new Repository<Payment>(_context);

	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await _context.SaveChangesAsync(cancellationToken);
	}

	public void Dispose()
	{
		_context?.Dispose();
	}
}
```

### 5. **Command Query Responsibility Segregation (CQRS)**

```csharp
// Commands - Write operations
public class CreateBookingCommand : IRequest<BookingDto>
{
	public int FlightId { get; set; }
	public int PassengerId { get; set; }
	public string PassengerName { get; set; }
}

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
{
	public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
	{
		// Business logic
	}
}

// Queries - Read operations
public class GetBookingQuery : IRequest<BookingDto>
{
	public int BookingId { get; set; }
}

public class GetBookingQueryHandler : IRequestHandler<GetBookingQuery, BookingDto>
{
	public async Task<BookingDto> Handle(GetBookingQuery request, CancellationToken cancellationToken)
	{
		// Query logic
	}
}

// Usage in Controller
[HttpPost]
public async Task<IActionResult> CreateBooking(CreateBookingCommand command)
{
	var result = await _mediator.Send(command);
	return Ok(result);
}
```

### 6. **Logging Strategy**

```csharp
// Use Serilog for structured logging
builder.Host.UseSerilog((context, configuration) =>
	configuration
		.MinimumLevel.Information()
		.WriteTo.Console()
		.WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
		.WriteTo.Seq("http://localhost:5341") // Centralized logging
		.Enrich.FromLogContext()
		.Enrich.WithProperty("Application", "FormulaAirline.Booking.Service")
		.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
);

// Usage
_logger.LogInformation("Booking created: {@Booking}", booking);
_logger.LogError(exception, "Booking creation failed for passenger {PassengerId}", passengerId);
```

### 7. **Exception Handling & Custom Exceptions**

```csharp
public abstract class DomainException : Exception
{
	public string Code { get; protected set; }
	protected DomainException(string message, string code = null) : base(message)
	{
		Code = code;
	}
}

public class FlightNotFoundException : DomainException
{
	public int FlightId { get; }
	public FlightNotFoundException(int flightId) 
		: base($"Flight with ID {flightId} not found", "FLIGHT_NOT_FOUND")
	{
		FlightId = flightId;
	}
}

public class InsufficientSeatsException : DomainException
{
	public InsufficientSeatsException() 
		: base("No available seats on this flight", "INSUFFICIENT_SEATS")
	{
	}
}

// Global Exception Handler Middleware
public class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unhandled exception occurred");
			await HandleExceptionAsync(context, exception);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		var response = new ErrorResponse();

		switch (exception)
		{
			case FlightNotFoundException ex:
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				response.Code = ex.Code;
				response.Message = ex.Message;
				break;

			case InsufficientSeatsException ex:
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				response.Code = ex.Code;
				response.Message = ex.Message;
				break;

			default:
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				response.Code = "INTERNAL_SERVER_ERROR";
				response.Message = "An unexpected error occurred";
				break;
		}

		return context.Response.WriteAsJsonAsync(response);
	}
}
```

### 8. **Validation with FluentValidation**

```csharp
public class CreateBookingValidator : AbstractValidator<CreateBookingRequest>
{
	public CreateBookingValidator()
	{
		RuleFor(x => x.FlightId)
			.GreaterThan(0)
			.WithMessage("Flight ID must be greater than 0");

		RuleFor(x => x.PassengerId)
			.GreaterThan(0)
			.WithMessage("Passenger ID must be greater than 0");

		RuleFor(x => x.PassengerName)
			.NotEmpty()
			.Length(2, 100)
			.WithMessage("Passenger name is required and must be 2-100 characters");

		RuleFor(x => x.Email)
			.EmailAddress()
			.WithMessage("Valid email is required");
	}
}

// Usage
public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
{
	private readonly IValidator<CreateBookingCommand> _validator;

	public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
	{
		var validationResult = await _validator.ValidateAsync(request, cancellationToken);
		if (!validationResult.IsValid)
			throw new ValidationException(validationResult.Errors);

		// Process booking
	}
}
```

### 9. **Resilience Patterns with Polly**

```csharp
// Install: dotnet add package Polly

public static IServiceCollection AddResiliencePolicies(this IServiceCollection services)
{
	// Retry policy
	var retryPolicy = Policy
		.Handle<HttpRequestException>()
		.Or<TimeoutException>()
		.WaitAndRetryAsync(
			retryCount: 3,
			sleepDurationProvider: retryAttempt =>
				TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
			onRetry: (outcome, timespan, retryCount, context) =>
			{
				// Log retry attempt
			}
		);

	// Circuit breaker policy
	var circuitBreakerPolicy = Policy
		.Handle<HttpRequestException>()
		.OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
		.CircuitBreakerAsync(
			handledEventsAllowedBeforeBreaking: 3,
			durationOfBreak: TimeSpan.FromSeconds(30),
			onBreak: (outcome, timespan) =>
			{
				// Log circuit break
			}
		);

	// Combine policies
	var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);

	return services;
}
```

### 10. **Async/Await Best Practices**

```csharp
// ❌ Wrong: Sync over async
public class BookingService
{
	public BookingDto CreateBooking(CreateBookingRequest request)
	{
		return CreateBookingAsync(request).Result; // Deadlock risk!
	}
}

// ✅ Correct: Async all the way
public class BookingService
{
	public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
	{
		var flight = await _flightService.GetFlightAsync(request.FlightId);
		var booking = new Booking(flight, request);
		await _repository.AddAsync(booking);
		return _mapper.Map<BookingDto>(booking);
	}
}

// ✅ Correct: Multiple operations
public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId)
{
	var bookings = await _repository.FindAsync(b => b.PassengerId == userId);
	var flights = await Task.WhenAll(
		bookings.Select(b => _flightService.GetFlightAsync(b.FlightId))
	);
	return _mapper.Map<IEnumerable<BookingDto>>(bookings);
}
```

---

## 📡 Asynchronous Communication Patterns

### Pattern 1: Event-Driven (Recommended for Learning)

```csharp
// Event Definition
public abstract class DomainEvent
{
	public Guid AggregateId { get; set; }
	public DateTime OccurredOn { get; set; }
	public int Version { get; set; }
}

public class BookingCreatedEvent : DomainEvent
{
	public int BookingId { get; set; }
	public int FlightId { get; set; }
	public int PassengerId { get; set; }
	public decimal Amount { get; set; }
}

// Event Publisher (RabbitMQ)
public interface IEventPublisher
{
	Task PublishAsync<T>(T @event) where T : DomainEvent;
}

public class RabbitMqEventPublisher : IEventPublisher
{
	private readonly IConnection _connection;
	private readonly IMapper _mapper;

	public async Task PublishAsync<T>(T @event) where T : DomainEvent
	{
		using var channel = _connection.CreateModel();

		var exchangeName = typeof(T).Name.Replace("Event", "");
		channel.ExchangeDeclare(exchange: exchangeName, type: "fanout", durable: true);

		var message = JsonConvert.SerializeObject(@event);
		var body = Encoding.UTF8.GetBytes(message);

		var properties = channel.CreateBasicProperties();
		properties.Persistent = true;

		channel.BasicPublish(
			exchange: exchangeName,
			routingKey: "",
			basicProperties: properties,
			body: body
		);

		await Task.CompletedTask;
	}
}

// Event Subscriber (Payment Service)
public class PaymentEventSubscriber : BackgroundService
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private IConnection _connection;
	private IModel _channel;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var factory = new ConnectionFactory { HostName = "localhost" };
		_connection = factory.CreateConnection();
		_channel = _connection.CreateModel();

		_channel.ExchangeDeclare(exchange: "BookingCreated", type: "fanout", durable: true);
		var queueName = _channel.QueueDeclare().QueueName;
		_channel.QueueBind(queue: queueName, exchange: "BookingCreated", routingKey: "");

		var consumer = new EventingBasicConsumer(_channel);
		consumer.Received += async (model, ea) =>
		{
			var message = Encoding.UTF8.GetString(ea.Body.ToArray());
			var bookingEvent = JsonConvert.DeserializeObject<BookingCreatedEvent>(message);

			using var scope = _serviceScopeFactory.CreateScope();
			var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
			await paymentService.ProcessPaymentAsync(bookingEvent);

			_channel.BasicAck(ea.DeliveryTag, false);
		};

		_channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
		await Task.Delay(Timeout.Infinite, stoppingToken);
	}
}
```

### Pattern 2: Request-Reply (Synchronous over Async)

```csharp
// Booking Service calling Payment Service directly
public class BookingService
{
	private readonly HttpClient _httpClient;

	public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
	{
		// Create booking
		var booking = new Booking(request);
		await _repository.AddAsync(booking);

		// Sync call to Payment Service (with retry & circuit breaker)
		var paymentRequest = new PaymentRequest { BookingId = booking.Id, Amount = booking.Amount };
		var paymentResponse = await _httpClient.PostAsJsonAsync("http://payment-service/payments", paymentRequest);

		if (!paymentResponse.IsSuccessStatusCode)
			throw new PaymentServiceException("Payment processing failed");

		return _mapper.Map<BookingDto>(booking);
	}
}
```

### Pattern 3: Saga Pattern (Distributed Transactions)

```csharp
// Orchestration-based Saga
public class BookingCreationSaga
{
	private readonly IFlightService _flightService;
	private readonly IPaymentService _paymentService;
	private readonly IEventPublisher _eventPublisher;

	public async Task<BookingDto> ExecuteAsync(CreateBookingRequest request)
	{
		var booking = new Booking(request);

		try
		{
			// Step 1: Reserve flight seats
			await _flightService.ReserveSeatsAsync(booking.FlightId, 1);

			// Step 2: Process payment (free for now, but structure in place)
			var paymentResult = await _paymentService.CreatePaymentAsync(booking);

			// Step 3: Confirm booking
			booking.Status = BookingStatus.Confirmed;
			await _repository.AddAsync(booking);

			// Step 4: Publish confirmation event
			await _eventPublisher.PublishAsync(new BookingConfirmedEvent(booking));

			return _mapper.Map<BookingDto>(booking);
		}
		catch (Exception ex)
		{
			// Compensate: Rollback changes
			await _flightService.ReleaseSeatAsync(booking.FlightId, 1);
			await _eventPublisher.PublishAsync(new BookingFailedEvent(booking));
			throw;
		}
	}
}
```

---

## 🗄️ Database Strategy for Microservices

### Database per Service Pattern

```
FormulaAirline.Flight.Service
├── Data/
│   ├── FlightDbContext.cs
│   ├── Migrations/
│   └── Connection: Server=flight-db;Database=FormulaAirlineFlights;
│
FormulaAirline.Booking.Service
├── Data/
│   ├── BookingDbContext.cs
│   ├── Migrations/
│   └── Connection: Server=booking-db;Database=FormulaAirlineBookings;
│
FormulaAirline.Payment.Service
├── Data/
│   ├── PaymentDbContext.cs
│   ├── Migrations/
│   └── Connection: Server=payment-db;Database=FormulaAirlinePayments;
```

**Benefits:**
- ✅ Independent scaling
- ✅ Technology flexibility
- ✅ Fault isolation
- ✅ Team autonomy

**Challenges:**
- ⚠️ Distributed transactions
- ⚠️ Data consistency
- ⚠️ Complex queries across services

---

## 🧪 Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class CreateBookingCommandHandlerTests
{
	private Mock<IFlightService> _mockFlightService;
	private Mock<IRepository<Booking>> _mockRepository;
	private CreateBookingCommandHandler _handler;

	[TestInitialize]
	public void Setup()
	{
		_mockFlightService = new Mock<IFlightService>();
		_mockRepository = new Mock<IRepository<Booking>>();
		_handler = new CreateBookingCommandHandler(_mockFlightService.Object, _mockRepository.Object);
	}

	[TestMethod]
	public async Task Handle_WithValidRequest_ReturnsBookingDto()
	{
		// Arrange
		var command = new CreateBookingCommand { FlightId = 1, PassengerId = 1 };
		_mockFlightService
			.Setup(x => x.GetFlightAsync(1))
			.ReturnsAsync(new Flight { Id = 1, AvailableSeats = 10 });

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		Assert.IsNotNull(result);
		_mockRepository.Verify(x => x.AddAsync(It.IsAny<Booking>()), Times.Once);
	}
}
```

### Integration Tests
```csharp
[TestClass]
public class BookingApiIntegrationTests
{
	private WebApplicationFactory<Program> _factory;
	private HttpClient _client;

	[TestInitialize]
	public void Setup()
	{
		_factory = new WebApplicationFactory<Program>();
		_client = _factory.CreateClient();
	}

	[TestMethod]
	public async Task PostBooking_WithValidRequest_Returns201Created()
	{
		// Arrange
		var request = new { FlightId = 1, PassengerId = 1 };

		// Act
		var response = await _client.PostAsJsonAsync("/api/bookings", request);

		// Assert
		Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
	}
}
```

---

## 🐳 Docker & Containerization

### Dockerfile for Each Service
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENTRYPOINT ["dotnet", "FormulaAirline.Booking.Service.dll"]
```

### docker-compose.yml for Local Development
```yaml
version: '3.8'

services:
  sqlserver:
	image: mcr.microsoft.com/mssql/server:2022-latest
	environment:
	  ACCEPT_EULA: Y
	  SA_PASSWORD: "YourPassword123!"
	ports:
	  - "1433:1433"

  rabbitmq:
	image: rabbitmq:3-management
	ports:
	  - "5672:5672"
	  - "15672:15672"

  redis:
	image: redis:latest
	ports:
	  - "6379:6379"

  flight-service:
	build:
	  context: ./FormulaAirline.Flight.Service
	  dockerfile: Dockerfile
	ports:
	  - "5001:80"
	depends_on:
	  - sqlserver
	  - rabbitmq
	environment:
	  ConnectionStrings__DefaultConnection: "Server=sqlserver;Initial Catalog=FormulaAirlineFlights;User Id=sa;Password=YourPassword123!"
	  RabbitMq__HostName: "rabbitmq"

  booking-service:
	build:
	  context: ./FormulaAirline.Booking.Service
	  dockerfile: Dockerfile
	ports:
	  - "5002:80"
	depends_on:
	  - sqlserver
	  - rabbitmq
	environment:
	  ConnectionStrings__DefaultConnection: "Server=sqlserver;Initial Catalog=FormulaAirlineBookings;User Id=sa;Password=YourPassword123!"
	  RabbitMq__HostName: "rabbitmq"

  payment-service:
	build:
	  context: ./FormulaAirline.Payment.Service
	  dockerfile: Dockerfile
	ports:
	  - "5003:80"
	depends_on:
	  - sqlserver
	  - rabbitmq
	environment:
	  ConnectionStrings__DefaultConnection: "Server=sqlserver;Initial Catalog=FormulaAirlinePayments;User Id=sa;Password=YourPassword123!"
	  RabbitMq__HostName: "rabbitmq"

  api-gateway:
	build:
	  context: ./FormulaAirline.Gateway
	  dockerfile: Dockerfile
	ports:
	  - "5000:80"
	depends_on:
	  - flight-service
	  - booking-service
	  - payment-service
```

---

## 📈 Performance & Scalability Tips

### 1. Caching Strategy
```csharp
public class CachedFlightService : IFlightService
{
	private readonly IFlightService _innerService;
	private readonly IDistributedCache _cache;

	public async Task<Flight> GetFlightAsync(int flightId)
	{
		var cacheKey = $"flight:{flightId}";
		var cachedFlight = await _cache.GetStringAsync(cacheKey);

		if (!string.IsNullOrEmpty(cachedFlight))
			return JsonConvert.DeserializeObject<Flight>(cachedFlight);

		var flight = await _innerService.GetFlightAsync(flightId);
		await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(flight), 
			new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

		return flight;
	}
}
```

### 2. Pagination for Large Result Sets
```csharp
public class PaginatedResult<T>
{
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalCount { get; set; }
	public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
	public IEnumerable<T> Data { get; set; }
}

public async Task<PaginatedResult<BookingDto>> GetUserBookingsAsync(int userId, int pageNumber = 1, int pageSize = 10)
{
	var skip = (pageNumber - 1) * pageSize;
	var bookings = await _repository
		.FindAsync(b => b.PassengerId == userId);

	var totalCount = bookings.Count();
	var data = bookings.Skip(skip).Take(pageSize);

	return new PaginatedResult<BookingDto>
	{
		PageNumber = pageNumber,
		PageSize = pageSize,
		TotalCount = totalCount,
		Data = _mapper.Map<IEnumerable<BookingDto>>(data)
	};
}
```

### 3. Database Indexing
```csharp
public class FlightDbContext : DbContext
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Index on frequently queried columns
		modelBuilder.Entity<Flight>()
			.HasIndex(f => f.Departure)
			.HasDatabaseName("idx_flight_departure");

		modelBuilder.Entity<Booking>()
			.HasIndex(b => b.PassengerId)
			.HasDatabaseName("idx_booking_passengerId");

		modelBuilder.Entity<Booking>()
			.HasIndex(b => new { b.PassengerId, b.Status })
			.HasDatabaseName("idx_booking_passenger_status");
	}
}
```

---

## 📊 Service Orchestration Map

```
┌─────────────────────────────────────────────────────────────────┐
│                     API Gateway (Ocelot)                        │
│              Single Entry Point for All Clients                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐ │
│  │  Flight Service  │  │  Booking Service │  │Payment Service
│  ├──────────────────┤  ├──────────────────┤  ├──────────────┤ │
│  │ - Browse flights │  │ - Create booking │  │- Process    │ │
│  │ - Check seats    │  │ - Cancel booking │  │  payment    │ │
│  │ - Reserve seats  │  │ - List bookings  │  │- Refund     │ │
│  └──────────────────┘  └──────────────────┘  └──────────────┘ │
│                                                                 │
│  ┌────────────────────────────────────────────────────────────┐│
│  │               RabbitMQ Message Bus (Event-Driven)          ││
│  │  - BookingCreated                                          ││
│  │  - BookingConfirmed                                        ││
│  │  - BookingCanceled                                         ││
│  │  - PaymentProcessed                                        ││
│  │  - PaymentFailed                                           ││
│  │  - FlightSeatsReserved                                     ││
│  │  - NotificationSent                                        ││
│  └────────────────────────────────────────────────────────────┘│
│                                                                 │
│  ┌──────────────────┐  ┌────────────────────┐                  │
│  │Notification      │  │User Service        │                  │
│  │Service           │  │                    │                  │
│  │ - Email/SMS      │  │- Auth              │                  │
│  │ - Push notif     │  │- Profile mgmt      │                  │
│  └──────────────────┘  └────────────────────┘                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

Database Layer (Separate per Service)
┌────────────┐  ┌────────────┐  ┌───────────┐  ┌────────────┐
│ Flight DB  │  │ Booking DB │  │ Payment DB│  │User DB     │
└────────────┘  └────────────┘  └───────────┘  └────────────┘
```

---

## 🚀 Learning Path & Milestones

### Week 1-2: Foundation
-Learn API Gateway (Ocelot)
- [ ] Set up Ocelot gateway
- [ ] Configure routing rules
- [ ] Test gateway endpoints

### Week 3: Message Bus
- [ ] Install RabbitMQ locally
- [ ] Publish/Subscribe pattern
- [ ] Dead letter queues

### Week 4: Refactor to Services
- [ ] Create Flight.Service project
- [ ] Create Booking.Service project
- [ ] Move code to respective services

### Week 5-6: Event-Driven Communication
- [ ] Event definitions
- [ ] Publish events from Booking Service
- [ ] Subscribe to events in Payment Service
- [ ] Handle event failures

### Week 7: Advanced Patterns
- [ ] CQRS implementation
- [ ] Saga pattern for distributed transactions
- [ ] Service resilience (Polly)

### Week 8-10: Testing & DevOps
- [ ] Unit tests
- [ ] Integration tests
- [ ] Docker containerization
- [ ] docker-compose setup

### Week 11-12: Deployment
- [ ] CI/CD pipeline
- [ ] Kubernetes introduction
- [ ] Health checks & monitoring

---

## 📚 Recommended Resources

### Books
1. **"Building Microservices" by Sam Newman** - Architecture patterns
2. **"Microservices Patterns" by Chris Richardson** - Design patterns
3. **"Domain-Driven Design" by Eric Evans** - Strategic design

### Online Courses
1. **Microsoft Learn** - microservices.io
2. **Pluralsight** - .NET Microservices Path
3. **Udemy** - Complete Microservices Course

### Tools & Technologies
- **API Gateway**: Ocelot, Kong, Azure API Management
- **Message Bus**: RabbitMQ, Azure Service Bus, Apache Kafka
- **Logging**: Serilog, ELK Stack, Application Insights
- **Monitoring**: Prometheus, Grafana, Datadog
- **Orchestration**: Kubernetes, Docker Compose
- **CI/CD**: GitHub Actions, Azure DevOps, Jenkins

---

## ✅ Quick Wins (Start Here!)

1. **Add Logging** (Serilog) - 30 minutes
2. **Implement Unit Tests** - 1 hour
3. **Add Validation** (FluentValidation) - 30 minutes
4. **Docker Compose** local setup - 1 hour
5. **Extract Services** to separate projects - 2 hours
6. **Set up Ocelot Gateway** - 1 hour

**Total Time to MVP Microservices**: ~2-3 weeks with daily effort

---

**Status**: 📚 Complete Learning Guide Ready
