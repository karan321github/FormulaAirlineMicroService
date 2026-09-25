# 🛠️ Step-by-Step Implementation Guide
## Phase 1: API Gateway & Service Foundation

---

## Step 1: Create API Gateway with Ocelot

### 1.1 Create New Project
```bash
dotnet new webapi -n FormulaAirline.Gateway
cd FormulaAirline.Gateway
dotnet add package Ocelot
dotnet add package Ocelot.Provider.Consul
dotnet add package Serilog.AspNetCore
```

### 1.2 Create ocelot.json Configuration

**File**: `FormulaAirline.Gateway/ocelot.json`

```json
{
  "Routes": [
	{
	  "DownstreamPathTemplate": "/api/flights/{everything}",
	  "DownstreamScheme": "http",
	  "DownstreamHostAndPorts": [
		{
		  "Host": "localhost",
		  "Port": 5001
		}
	  ],
	  "UpstreamPathTemplate": "/flights/{everything}",
	  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
	  "RateLimitOptions": {
		"ClientWhitelist": [],
		"EnableRateLimiting": true,
		"Period": "1s",
		"PeriodTimespan": 1,
		"Limit": 10
	  }
	},
	{
	  "DownstreamPathTemplate": "/api/bookings/{everything}",
	  "DownstreamScheme": "http",
	  "DownstreamHostAndPorts": [
		{
		  "Host": "localhost",
		  "Port": 5002
		}
	  ],
	  "UpstreamPathTemplate": "/bookings/{everything}",
	  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
	},
	{
	  "DownstreamPathTemplate": "/api/payments/{everything}",
	  "DownstreamScheme": "http",
	  "DownstreamHostAndPorts": [
		{
		  "Host": "localhost",
		  "Port": 5003
		}
	  ],
	  "UpstreamPathTemplate": "/payments/{everything}",
	  "UpstreamHttpMethod": [ "GET", "POST" ]
	}
  ],
  "GlobalConfiguration": {
	"BaseUrl": "http://localhost:5000",
	"RateLimitOptions": {
	  "DisableRateLimitHeaders": false,
	  "QuotaExceededMessage": "API Rate limit exceeded",
	  "HttpStatusCode": 429
	}
  }
}
```

### 1.3 Update Program.cs

```csharp
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot
builder.Services.AddOcelot(builder.Configuration);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
	configuration
		.MinimumLevel.Information()
		.WriteTo.Console()
		.Enrich.FromLogContext()
);

builder.Services.AddLogging();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseRouting();

// Use Ocelot
app.UseOcelot().Wait();

app.Run();
```

---

## Step 2: Refactor Current API to Flight Service

### 2.1 Create Flight Service Project
```bash
dotnet new webapi -n FormulaAirline.Flight.Service
cd FormulaAirline.Flight.Service
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package AutoMapper
dotnet add package Serilog.AspNetCore
dotnet add package RabbitMQ.Client
```

### 2.2 Create Flight Service Structure

```
FormulaAirline.Flight.Service/
├── Controllers/
│   └── FlightsController.cs
├── Services/
│   ├── IFlightService.cs
│   └── FlightService.cs
├── Models/
│   └── Flight.cs
├── DTOs/
│   └── FlightDto.cs
├── Data/
│   ├── FlightDbContext.cs
│   └── Migrations/
├── Events/
│   └── FlightEvents.cs
├── Mappings/
│   └── MappingProfile.cs
└── Program.cs
```

### 2.3 Flight Service Program.cs

```csharp
using FormulaAirline.Flight.Service.Data;
using FormulaAirline.Flight.Service.Mappings;
using FormulaAirline.Flight.Service.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
builder.Host.UseSerilog((context, configuration) =>
	configuration
		.MinimumLevel.Information()
		.WriteTo.Console()
		.WriteTo.File("logs/flight-service.log", rollingInterval: RollingInterval.Day)
		.Enrich.FromLogContext()
		.Enrich.WithProperty("Application", "FormulaAirline.Flight.Service")
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
	?? "Server=(localdb)\\mssqllocaldb;Database=FormulaAirlineFlights;Trusted_Connection=true;";
builder.Services.AddDbContext<FlightDbContext>(options =>
	options.UseSqlServer(connectionString)
);

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Services
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

// Migration
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
	dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run("http://localhost:5001");
```

### 2.4 FlightDto.cs

```csharp
namespace FormulaAirline.Flight.Service.DTOs
{
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
	}

	public class CreateFlightRequest
	{
		public string FlightNumber { get; set; }
		public string Departure { get; set; }
		public string Arrival { get; set; }
		public int Capacity { get; set; }
		public DateTime DepartureTime { get; set; }
		public DateTime ArrivalTime { get; set; }
		public decimal Price { get; set; }
	}
}
```

### 2.5 Domain Events for Flight Service

```csharp
namespace FormulaAirline.Flight.Service.Events
{
	public abstract class DomainEvent
	{
		public Guid EventId { get; set; } = Guid.NewGuid();
		public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
	}

	public class FlightSeatsReservedEvent : DomainEvent
	{
		public int FlightId { get; set; }
		public int SeatsReserved { get; set; }
		public int RemainingSeats { get; set; }
	}

	public class FlightSeatsReleasedEvent : DomainEvent
	{
		public int FlightId { get; set; }
		public int SeatsReleased { get; set; }
		public int RemainingSeats { get; set; }
	}
}
```

---

## Step 3: Create Booking Service

### 3.1 Project Structure
```bash
dotnet new webapi -n FormulaAirline.Booking.Service
```

### 3.2 Booking Service Program.cs

```csharp
using FormulaAirline.Booking.Service.Data;
using FormulaAirline.Booking.Service.Mappings;
using FormulaAirline.Booking.Service.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
	configuration
		.MinimumLevel.Information()
		.WriteTo.Console()
		.WriteTo.File("logs/booking-service.log", rollingInterval: RollingInterval.Day)
		.Enrich.FromLogContext()
		.Enrich.WithProperty("Application", "FormulaAirline.Booking.Service")
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<BookingDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// HTTP Client for calling Flight Service
builder.Services.AddHttpClient("FlightService", client =>
{
	client.BaseAddress = new Uri("http://localhost:5001");
	client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Migration
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
	dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run("http://localhost:5002");
```

### 3.3 Booking Service with HTTP Communication

```csharp
namespace FormulaAirline.Booking.Service.Services
{
	public interface IBookingService
	{
		Task<BookingDto> CreateBookingAsync(CreateBookingRequest request);
		Task<BookingDto> GetBookingAsync(int bookingId);
		Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int passengerId);
	}

	public class BookingService : IBookingService
	{
		private readonly IRepository<Booking> _bookingRepository;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMapper _mapper;
		private readonly ILogger<BookingService> _logger;

		public BookingService(
			IRepository<Booking> bookingRepository,
			IHttpClientFactory httpClientFactory,
			IMapper mapper,
			ILogger<BookingService> logger)
		{
			_bookingRepository = bookingRepository;
			_httpClientFactory = httpClientFactory;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
		{
			try
			{
				// Step 1: Validate flight exists via HTTP call to Flight Service
				var client = _httpClientFactory.CreateClient("FlightService");
				var flightResponse = await client.GetAsync($"/api/flights/{request.FlightId}");

				if (!flightResponse.IsSuccessStatusCode)
				{
					_logger.LogWarning("Flight {FlightId} not found", request.FlightId);
					throw new FlightNotFoundException($"Flight with ID {request.FlightId} not found");
				}

				var flightContent = await flightResponse.Content.ReadAsStringAsync();
				var flightDto = JsonConvert.DeserializeObject<FlightDto>(flightContent);

				if (flightDto.AvailableSeats < 1)
				{
					_logger.LogWarning("No available seats for flight {FlightId}", request.FlightId);
					throw new InsufficientSeatsException();
				}

				// Step 2: Create booking
				var booking = new Booking
				{
					FlightId = request.FlightId,
					PassengerId = request.PassengerId,
					PassangerName = request.PassengerName,
					PassportNb = request.PassportNb,
					From = request.From,
					To = request.To,
					Status = BookingStatus.Pending,
					CreatedAt = DateTime.UtcNow
				};

				await _bookingRepository.AddAsync(booking);

				_logger.LogInformation("Booking created: {BookingId} for passenger {PassengerId}", 
					booking.Id, request.PassengerId);

				// Step 3: Publish event (we'll implement this later with RabbitMQ)
				// await _eventPublisher.PublishAsync(new BookingCreatedEvent(booking));

				return _mapper.Map<BookingDto>(booking);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating booking for passenger {PassengerId}", request.PassengerId);
				throw;
			}
		}

		public async Task<BookingDto> GetBookingAsync(int bookingId)
		{
			var booking = await _bookingRepository.GetByIdAsync(bookingId);
			if (booking == null)
				throw new BookingNotFoundException(bookingId);

			return _mapper.Map<BookingDto>(booking);
		}

		public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int passengerId)
		{
			var bookings = await _bookingRepository.FindAsync(b => b.PassengerId == passengerId);
			return _mapper.Map<IEnumerable<BookingDto>>(bookings);
		}
	}
}
```

---

## Step 4: Implement RabbitMQ Event Publishing

### 4.1 Event Bus Service

```csharp
namespace FormulaAirline.Shared.EventBus
{
	// Event definitions
	public abstract class IntegrationEvent
	{
		public Guid Id { get; } = Guid.NewGuid();
		public DateTime CreatedTime { get; } = DateTime.UtcNow;
	}

	public class BookingCreatedIntegrationEvent : IntegrationEvent
	{
		public int BookingId { get; set; }
		public int FlightId { get; set; }
		public int PassengerId { get; set; }
		public string PassengerName { get; set; }
		public decimal Price { get; set; }
	}

	public class PaymentProcessedIntegrationEvent : IntegrationEvent
	{
		public int BookingId { get; set; }
		public int PaymentId { get; set; }
		public decimal Amount { get; set; }
		public bool Success { get; set; }
		public string Message { get; set; }
	}

	// Event publisher interface
	public interface IEventBusPublisher
	{
		Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event) 
			where TIntegrationEvent : IntegrationEvent;
	}

	// RabbitMQ implementation
	public class RabbitMqEventPublisher : IEventBusPublisher
	{
		private readonly IConnection _connection;
		private readonly ILogger<RabbitMqEventPublisher> _logger;

		public RabbitMqEventPublisher(IConnection connection, ILogger<RabbitMqEventPublisher> logger)
		{
			_connection = connection;
			_logger = logger;
		}

		public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event) 
			where TIntegrationEvent : IntegrationEvent
		{
			try
			{
				using var channel = _connection.CreateModel();

				var exchangeName = "formulaairline_event_bus";
				var routingKey = @event.GetType().Name;

				channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);

				var message = JsonConvert.SerializeObject(@event);
				var body = Encoding.UTF8.GetBytes(message);

				var properties = channel.CreateBasicProperties();
				properties.Persistent = true;
				properties.ContentType = "application/json";

				channel.BasicPublish(exchangeName, routingKey, properties, body);

				_logger.LogInformation("Event published: {EventType} with Id: {EventId}", 
					routingKey, @event.Id);

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error publishing event: {EventType}", 
					@event.GetType().Name);
				throw;
			}
		}
	}
}
```

### 4.2 Event Subscriber

```csharp
namespace FormulaAirline.Payment.Service.EventHandlers
{
	public class BookingCreatedEventHandler : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IConnection _connection;
		private readonly ILogger<BookingCreatedEventHandler> _logger;
		private IModel _channel;

		public BookingCreatedEventHandler(
			IServiceScopeFactory serviceScopeFactory,
			IConnection connection,
			ILogger<BookingCreatedEventHandler> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_connection = connection;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				_channel = _connection.CreateModel();

				var exchangeName = "formulaairline_event_bus";
				var queueName = "payment_service_queue";
				var routingKey = nameof(BookingCreatedIntegrationEvent);

				_channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
				_channel.QueueDeclare(queueName, durable: true, autoDelete: false);
				_channel.QueueBind(queueName, exchangeName, routingKey);

				var consumer = new EventingBasicConsumer(_channel);
				consumer.Received += async (model, ea) =>
				{
					try
					{
						var message = Encoding.UTF8.GetString(ea.Body.ToArray());
						var bookingCreatedEvent = JsonConvert.DeserializeObject<BookingCreatedIntegrationEvent>(message);

						_logger.LogInformation("Processing BookingCreatedEvent: {BookingId}", bookingCreatedEvent.BookingId);

						using var scope = _serviceScopeFactory.CreateScope();
						var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
						await paymentService.ProcessBookingPaymentAsync(bookingCreatedEvent);

						_channel.BasicAck(ea.DeliveryTag, false);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error processing BookingCreatedEvent");
						_channel.BasicNack(ea.DeliveryTag, false, true); // Requeue
					}
				};

				_channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

				_logger.LogInformation("BookingCreatedEventHandler started listening");
				await Task.Delay(Timeout.Infinite, stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in BookingCreatedEventHandler");
			}
		}

		public override void Dispose()
		{
			_channel?.Dispose();
			base.Dispose();
		}
	}
}
```

### 4.3 Register in Program.cs

```csharp
// In Booking Service Program.cs
builder.Services.AddSingleton(new ConnectionFactory 
{ 
	HostName = builder.Configuration["RabbitMq:HostName"] ?? "localhost" 
}.CreateConnection());

builder.Services.AddSingleton<IEventBusPublisher, RabbitMqEventPublisher>();
builder.Services.AddHostedService<BookingCreatedEventHandler>();

// In Booking Service - Update CreateBookingAsync
public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
{
	// ... existing code ...

	// Publish event
	var bookingCreatedEvent = new BookingCreatedIntegrationEvent
	{
		BookingId = booking.Id,
		FlightId = booking.FlightId,
		PassengerId = booking.PassengerId,
		PassengerName = booking.PassangerName,
		Price = 0 // Free booking
	};

	await _eventPublisher.PublishAsync(bookingCreatedEvent);

	// ... rest of code ...
}
```

---

## Step 5: Create Payment Service

### 5.1 Payment Service Handler

```csharp
namespace FormulaAirline.Payment.Service.Services
{
	public interface IPaymentService
	{
		Task ProcessBookingPaymentAsync(BookingCreatedIntegrationEvent bookingEvent);
	}

	public class PaymentService : IPaymentService
	{
		private readonly IRepository<Payment> _paymentRepository;
		private readonly IEventBusPublisher _eventPublisher;
		private readonly ILogger<PaymentService> _logger;

		public async Task ProcessBookingPaymentAsync(BookingCreatedIntegrationEvent bookingEvent)
		{
			try
			{
				_logger.LogInformation("Processing payment for booking {BookingId}", bookingEvent.BookingId);

				// Create payment record
				var payment = new Payment
				{
					BookingId = bookingEvent.BookingId,
					PassengerId = bookingEvent.PassengerId,
					Amount = bookingEvent.Price, // 0 for free platform
					Status = PaymentStatus.Completed, // Auto-complete for free bookings
					PaymentMethod = "Free",
					TransactionId = Guid.NewGuid().ToString(),
					PaymentDate = DateTime.UtcNow
				};

				await _paymentRepository.AddAsync(payment);

				_logger.LogInformation("Payment created: {PaymentId} for booking {BookingId}", 
					payment.Id, bookingEvent.BookingId);

				// Publish payment processed event
				var paymentProcessedEvent = new PaymentProcessedIntegrationEvent
				{
					BookingId = bookingEvent.BookingId,
					PaymentId = payment.Id,
					Amount = payment.Amount,
					Success = true,
					Message = "Payment processed successfully"
				};

				await _eventPublisher.PublishAsync(paymentProcessedEvent);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing payment for booking {BookingId}", bookingEvent.BookingId);

				var paymentFailedEvent = new PaymentProcessedIntegrationEvent
				{
					BookingId = bookingEvent.BookingId,
					Success = false,
					Message = $"Payment processing failed: {ex.Message}"
				};

				await _eventPublisher.PublishAsync(paymentFailedEvent);
			}
		}
	}
}
```

---

## Step 6: Docker Compose Setup

### 6.1 docker-compose.yml

```yaml
version: '3.8'

services:
  sqlserver:
	image: mcr.microsoft.com/mssql/server:2022-latest
	container_name: formulaairline_mssql
	environment:
	  ACCEPT_EULA: "Y"
	  SA_PASSWORD: "FormulaAirline@2024"
	  MSSQL_PID: "Express"
	ports:
	  - "1433:1433"
	volumes:
	  - sqlserver_data:/var/opt/mssql
	networks:
	  - formulaairline_network

  rabbitmq:
	image: rabbitmq:3.12-management
	container_name: formulaairline_rabbitmq
	environment:
	  RABBITMQ_DEFAULT_USER: "guest"
	  RABBITMQ_DEFAULT_PASS: "guest"
	ports:
	  - "5672:5672"
	  - "15672:15672"
	volumes:
	  - rabbitmq_data:/var/lib/rabbitmq
	networks:
	  - formulaairline_network

  redis:
	image: redis:7-alpine
	container_name: formulaairline_redis
	ports:
	  - "6379:6379"
	volumes:
	  - redis_data:/data
	networks:
	  - formulaairline_network

  gateway:
	build:
	  context: .
	  dockerfile: FormulaAirline.Gateway/Dockerfile
	container_name: formulaairline_gateway
	ports:
	  - "5000:80"
	depends_on:
	  - flight-service
	  - booking-service
	  - payment-service
	environment:
	  - ASPNETCORE_ENVIRONMENT=Docker
	networks:
	  - formulaairline_network

  flight-service:
	build:
	  context: .
	  dockerfile: FormulaAirline.Flight.Service/Dockerfile
	container_name: formulaairline_flight_service
	ports:
	  - "5001:80"
	depends_on:
	  - sqlserver
	  - rabbitmq
	environment:
	  - ASPNETCORE_ENVIRONMENT=Docker
	  - ConnectionStrings__DefaultConnection=Server=sqlserver;Initial Catalog=FormulaAirlineFlights;User Id=sa;Password=FormulaAirline@2024;TrustServerCertificate=true;
	  - RabbitMq__HostName=rabbitmq
	networks:
	  - formulaairline_network

  booking-service:
	build:
	  context: .
	  dockerfile: FormulaAirline.Booking.Service/Dockerfile
	container_name: formulaairline_booking_service
	ports:
	  - "5002:80"
	depends_on:
	  - sqlserver
	  - rabbitmq
	  - flight-service
	environment:
	  - ASPNETCORE_ENVIRONMENT=Docker
	  - ConnectionStrings__DefaultConnection=Server=sqlserver;Initial Catalog=FormulaAirlineBookings;User Id=sa;Password=FormulaAirline@2024;TrustServerCertificate=true;
	  - RabbitMq__HostName=rabbitmq
	  - Services__FlightService=http://flight-service
	networks:
	  - formulaairline_network

  payment-service:
	build:
	  context: .
	  dockerfile: FormulaAirline.Payment.Service/Dockerfile
	container_name: formulaairline_payment_service
	ports:
	  - "5003:80"
	depends_on:
	  - sqlserver
	  - rabbitmq
	environment:
	  - ASPNETCORE_ENVIRONMENT=Docker
	  - ConnectionStrings__DefaultConnection=Server=sqlserver;Initial Catalog=FormulaAirlinePayments;User Id=sa;Password=FormulaAirline@2024;TrustServerCertificate=true;
	  - RabbitMq__HostName=rabbitmq
	networks:
	  - formulaairline_network

volumes:
  sqlserver_data:
  rabbitmq_data:
  redis_data:

networks:
  formulaairline_network:
	driver: bridge
```

### 6.2 Dockerfile for Each Service

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the entire solution
COPY . .

# Build
RUN dotnet restore
RUN dotnet build -c Release --no-restore

# Publish
RUN dotnet publish -c Release -o /app/publish --no-build

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "FormulaAirline.Flight.Service.dll"]
```

---

## Quick Start Commands

```bash
# Build all services
docker-compose build

# Start all services
docker-compose up

# Stop all services
docker-compose down

# View logs
docker-compose logs -f booking-service

# Access RabbitMQ Management
# http://localhost:15672
# Default credentials: guest/guest

# Access Services
# Gateway: http://localhost:5000
# Flight Service: http://localhost:5001
# Booking Service: http://localhost:5002
# Payment Service: http://localhost:5003
```

---

## Next Steps

1. **Unit Tests** - Create test projects for each service
2. **Integration Tests** - Test service-to-service communication
3. **CI/CD** - Set up GitHub Actions or Azure DevOps
4. **Kubernetes** - Deploy to K8s cluster
5. **Monitoring** - Add Prometheus & Grafana

**Status**: 🚀 Ready to implement Phase 1-3 (Foundation to Event-Driven Communication)
