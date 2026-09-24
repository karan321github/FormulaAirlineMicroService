# Project Structure Overview

## Folder Organization

```
FormulaAirlineMicroService/
├── formulaAirline.Api/                          # Main API Project
│   ├── Controllers/
│   │   ├── BookingController.cs                 # Booking endpoints (CRUD + validation)
│   │   ├── FlightController.cs                  # Flight management endpoints
│   │   ├── PaymentController.cs                 # Payment processing endpoints
│   │   └── WeatherForecastController.cs         # Sample controller
│   │
│   ├── Models/
│   │   ├── Booking.cs                           # Booking entity with relationships
│   │   ├── Flight.cs                            # Flight entity
│   │   ├── Payment.cs                           # Payment entity
│   │   └── WeatherForecast.cs                   # Sample model
│   │
│   ├── Services/
│   │   ├── IFlightService.cs                    # Flight service interface
│   │   ├── FlightService.cs                     # Flight business logic
│   │   ├── IPaymentService.cs                   # Payment service interface
│   │   ├── PaymentService.cs                    # Payment business logic
│   │   ├── IMessageProducer.cs                  # Message queue interface
│   │   └── MessageProducer.cs                   # Message queue implementation
│   │
│   ├── Repository/
│   │   ├── IRepository.cs                       # Generic repository interface
│   │   └── Repository.cs                        # Generic repository implementation
│   │
│   ├── Data/
│   │   └── ApplicationDbContext.cs              # EF Core DbContext
│   │
│   ├── Migrations/
│   │   ├── 20260924143444_InitialMigration.cs           # Migration script
│   │   ├── 20260924143444_InitialMigration.Designer.cs  # Migration metadata
│   │   └── ApplicationDbContextModelSnapshot.cs         # Current model state
│   │
│   ├── Properties/
│   │   └── launchSettings.json                  # Launch configuration
│   │
│   ├── Program.cs                               # Startup configuration
│   ├── appsettings.json                         # App settings (connection string)
│   ├── appsettings.Development.json             # Dev settings
│   ├── formulaAirline.Api.csproj               # Project file
│   └── formulaAirline.Api.http                 # HTTP test file
│
├── formulaAirline.ticketProcessing/             # Microservice project
│   ├── Program.cs
│   └── formulaAirline.ticketProcessing.csproj
│
├── FormulaAirline.sln                           # Solution file
└── README.md, etc.
```

## Key Components

### 1. Models Directory (Data Layer)
- **Booking.cs**: Passenger booking with flight reference
  - Properties: Id, PassangerName, PassportNb, From, To, Status, FlightId
  - Relationships: One-to-Many with Payments, Many-to-One with Flight

- **Flight.cs**: Available flights
  - Properties: Id, FlightNumber, Departure, Arrival, Capacity, AvailableSeats, Times, Price
  - Relationships: One-to-Many with Bookings

- **Payment.cs**: Payment records
  - Properties: Id, BookingId, Amount, Status, PaymentMethod, TransactionId, PaymentDate
  - Relationships: Many-to-One with Booking

### 2. Controllers Directory (API Layer)
- **BookingController**: REST endpoints for booking management
  - GET /Booking - List all
  - GET /Booking/{id} - Get by ID
  - GET /Booking/passenger/{name} - Search by passenger
  - POST /Booking - Create with validation
  - PUT /Booking/{id} - Update
  - DELETE /Booking/{id} - Delete

- **FlightController**: REST endpoints for flight management
  - GET /Flight - List all
  - GET /Flight/{id} - Get by ID
  - GET /Flight/search - Search by route
  - GET /Flight/{id}/check-availability - Seat check
  - POST /Flight - Create
  - PUT /Flight/{id} - Update
  - DELETE /Flight/{id} - Delete

- **PaymentController**: REST endpoints for payment processing
  - GET /Payment - List all
  - GET /Payment/{id} - Get by ID
  - GET /Payment/booking/{id} - Get for booking
  - GET /Payment/booking/{id}/total - Total amount
  - POST /Payment - Create
  - POST /Payment/process - Process payment
  - PUT /Payment/{id} - Update
  - DELETE /Payment/{id} - Delete

### 3. Services Directory (Business Logic Layer)
- **IFlightService / FlightService**: 
  - Flight CRUD operations
  - Seat availability checking
  - Seat reservation logic
  - Search functionality

- **IPaymentService / PaymentService**:
  - Payment CRUD operations
  - Payment processing with validation
  - Total calculation
  - Message queue integration

- **IMessageProducer / MessageProducer**:
  - Event publishing (existing service)
  - Used for async notifications

### 4. Repository Directory (Data Access Layer)
- **IRepository<T>**: Generic interface for CRUD operations
  - GetByIdAsync, GetAllAsync, FindAsync
  - AddAsync, UpdateAsync, DeleteAsync
  - AnyAsync, CountAsync, SaveChangesAsync

- **Repository<T>**: Generic implementation
  - Works with any entity type
  - Uses EF Core DbSet operations
  - Exception handling and logging

### 5. Data Directory (Database Layer)
- **ApplicationDbContext**: EF Core context
  - DbSets for all entities
  - OnModelCreating for entity configuration
  - Relationships and constraints
  - Default configuration for SQL Server

### 6. Migrations Directory (Database Schema)
- **InitialMigration**: Creates database schema
  - Bookings table
  - Flights table
  - Payments table
  - Foreign keys and relationships
  - Constraints and indexes

## Architecture Pattern

```
API Request
	↓
[Controller] - Handles HTTP
	↓
[Service] - Business Logic
	↓
[Repository] - Data Access
	↓
[DbContext] - Entity Framework
	↓
[SQL Server] - Database
```

## Dependency Injection Container

Registered in Program.cs:
```
- DbContext: ApplicationDbContext
- Repository: IRepository<T> → Repository<T> (scoped)
- Services: IFlightService → FlightService (scoped)
- Services: IPaymentService → PaymentService (scoped)
- Services: IMessageProducer → MessageProducer (scoped)
```

## Data Flow Example: Create Booking

1. **HTTP Request**: POST /Booking with booking data
2. **Controller**: BookingController.CreatingBooking()
   - Validates ModelState
   - Calls FlightService to check availability
   - Calls FlightService to reserve seat
   - Creates booking object
3. **Repository**: IRepository<Booking>.AddAsync()
   - Adds entity to DbSet
   - Calls SaveChangesAsync
4. **DbContext**: ApplicationDbContext
   - Change tracking
   - SQL generation
5. **Database**: SQL Server
   - INSERT into Bookings table
   - UPDATE Flights.AvailableSeats
6. **Service**: MessageProducer
   - Publishes booking event
7. **Response**: Returns 201 Created with booking ID

## Configuration Files

### appsettings.json
- Connection strings
- Logging levels
- API configuration

### launchSettings.json
- Debug/Release profiles
- Port configuration
- URL bindings

## NuGet Dependencies

- **Microsoft.EntityFrameworkCore**: ORM framework
- **Microsoft.EntityFrameworkCore.SqlServer**: SQL Server provider
- **Microsoft.EntityFrameworkCore.Design**: Migration tools
- **Microsoft.AspNetCore.Mvc**: Web API framework
- **Microsoft.Extensions.Logging**: Logging

## Testing Entry Points

For unit testing, mock these interfaces:
- `IRepository<T>`
- `IFlightService`
- `IPaymentService`
- `IMessageProducer`
- `ILogger<T>`

## Security Considerations

Current implementation includes:
- Model validation
- Exception handling
- Logging for audit trail
- Foreign key constraints

Recommended additions:
- Authentication (JWT/OAuth)
- Authorization (Role-based access control)
- Input sanitization
- Rate limiting
- HTTPS enforcement

## Performance Considerations

Current optimizations:
- Async/await for non-blocking I/O
- Repository pattern for data access
- Connection pooling via EF Core
- Generic repository for reusability

Recommended improvements:
- Caching (Redis) for frequent queries
- Database indexing on foreign keys
- Query optimization with Select projections
- Pagination for list endpoints
- Background job processing for payments

---

## File Statistics

| Component | Files | LOC (avg) |
|-----------|-------|----------|
| Controllers | 3 | ~150 |
| Services | 4 | ~200 |
| Models | 3 | ~50 |
| Repository | 2 | ~100 |
| DbContext | 1 | ~80 |
| Migrations | 3 | ~500 |
| **Total** | **16** | **~1,080** |

---

## Development Workflow

1. **Clone Repository**
   ```bash
   git clone https://github.com/karan321github/FormulaAirlineMicroService
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Update Database**
   ```bash
   dotnet ef database update
   ```

4. **Run Application**
   ```bash
   dotnet run --project formulaAirline.Api
   ```

5. **Access APIs**
   - Swagger: https://localhost:5001/swagger
   - Health: https://localhost:5001/health

---
