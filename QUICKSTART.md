# Quick Start Guide

## Prerequisites

- **.NET 8 SDK** or higher
- **Visual Studio 2022** (or VS Code)
- **SQL Server LocalDB** (or SQL Server instance)
- **Git**

## Installation Steps

### 1. Clone the Repository
```bash
cd C:\Users\YourUsername\source\repos
git clone https://github.com/karan321github/FormulaAirlineMicroService
cd FormulaAirlineMicroService
```

### 2. Restore NuGet Packages
```bash
dotnet restore
```

### 3. Update Database Connection (Optional)
Edit `formulaAirline.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FormulaAirlineDb;Trusted_Connection=true;"
  }
}
```

### 4. Build the Solution
```bash
dotnet build
```

### 5. Apply Database Migrations
```bash
cd formulaAirline.Api
dotnet ef database update
```

### 6. Run the Application
```bash
dotnet run
```

### 7. Access the Application
- **Swagger UI**: https://localhost:5001/swagger
- **API Base URL**: https://localhost:5001

---

## Quick Test Flow

### Step 1: Create a Flight (Swagger)
1. Open Swagger UI
2. Go to Flight Controller
3. Click "Try it out" on POST /Flight
4. Enter sample data:
```json
{
  "flightNumber": "FA101",
  "departure": "New York",
  "arrival": "London",
  "capacity": 180,
  "departureTime": "2024-12-25T10:00:00Z",
  "arrivalTime": "2024-12-25T22:00:00Z",
  "price": 750.00
}
```
5. Click "Execute"
6. Note the returned Flight ID

### Step 2: Create a Booking
1. Go to Booking Controller
2. Click "Try it out" on POST /Booking
3. Enter sample data:
```json
{
  "pasangerName": "John Doe",
  "passportNb": "AB123456",
  "from": "New York",
  "to": "London",
  "flightId": 1
}
```
4. Click "Execute"
5. Note the returned Booking ID

### Step 3: Process Payment
1. Go to Payment Controller
2. Click "Try it out" on POST /Payment/process
3. Enter sample data:
```json
{
  "bookingId": 1,
  "amount": 750.00,
  "paymentMethod": "CreditCard"
}
```
4. Click "Execute"

### Step 4: View Booking Details
1. Go to Booking Controller
2. Click "Try it out" on GET /Booking/{id}
3. Enter ID: 1
4. Click "Execute"
5. View detailed booking information

### Step 5: Get Total Payment
1. Go to Payment Controller
2. Click "Try it out" on GET /Payment/booking/{bookingId}/total
3. Enter bookingId: 1
4. Click "Execute"
5. View total payment amount

---

## Using cURL Commands

### Create Flight
```bash
curl -X POST https://localhost:5001/Flight \
  -H "Content-Type: application/json" \
  -d @flight.json
```

### Create Booking
```bash
curl -X POST https://localhost:5001/Booking \
  -H "Content-Type: application/json" \
  -d @booking.json
```

### Process Payment
```bash
curl -X POST https://localhost:5001/Payment/process \
  -H "Content-Type: application/json" \
  -d @payment.json
```

### Get All Bookings
```bash
curl -X GET https://localhost:5001/Booking
```

---

## Visual Studio Workflow

### 1. Open Solution
```
File → Open → Project/Solution
→ FormulaAirline.sln
```

### 2. Set Startup Project
- Right-click `formulaAirline.Api`
- Select "Set as Startup Project"

### 3. Run Project
- Press `F5` or click "Run"
- Visual Studio will launch the debugger
- Swagger UI opens automatically

### 4. Debug
- Set breakpoints by clicking line numbers
- Step through code with F10/F11
- Inspect variables in Watch window

### 5. View Database
- Open SQL Server Object Explorer
- Connect to `(localdb)\mssqllocaldb`
- Expand `FormulaAirlineDb`
- View tables: Bookings, Flights, Payments

---

## Verify Installation

### Check Database Created
```bash
# Open PowerShell
sqlcmd -S "(localdb)\mssqllocaldb"

# Run query
SELECT name FROM sys.databases WHERE name = 'FormulaAirlineDb'
GO
```

### Check Migrations Applied
```bash
# In formulaAirline.Api directory
dotnet ef migrations list
```

Output should show:
```
20260924143444_InitialMigration
```

### Test API Health
```bash
curl -X GET https://localhost:5001/Flight
```

Should return empty array:
```json
[]
```

---

## Troubleshooting

### Issue: Connection String Error
**Solution**: Verify SQL Server LocalDB is installed
```bash
sqllocaldb info
sqllocaldb start mssqllocaldb
```

### Issue: Migration Failed
**Solution**: Drop and recreate database
```bash
dotnet ef database drop -f
dotnet ef database update
```

### Issue: Port Already in Use
**Solution**: Change port in `launchSettings.json`
```json
"applicationUrl": "https://localhost:5002"
```

### Issue: HTTPS Certificate Error
**Solution**: Trust the certificate
```bash
dotnet dev-certs https --trust
```

### Issue: Build Fails
**Solution**: Clean and rebuild
```bash
dotnet clean
dotnet build
```

---

## Project Statistics

| Metric | Value |
|--------|-------|
| Controllers | 3 (Booking, Flight, Payment) |
| Services | 2 (Flight, Payment) |
| Models | 3 (Booking, Flight, Payment) |
| Database Tables | 3 |
| API Endpoints | 18+ |
| Lines of Code | ~1,500 |

---

## Key Features Implemented

✅ **Database Integration**
- SQL Server with Entity Framework Core
- LocalDB by default
- Automatic migrations on startup

✅ **Flight Management Service**
- Create, read, update, delete flights
- Search flights by route
- Track seat availability
- Seat reservation system

✅ **Payment Processing Service**
- Record payments
- Process payments with validation
- Calculate totals
- Message queue integration

✅ **Booking Management**
- Create bookings with validation
- Check flight availability
- Reserve seats automatically
- Search by passenger
- Full CRUD operations

✅ **Architecture**
- Repository pattern for data access
- Dependency injection
- Async/await operations
- Comprehensive error handling
- Structured logging

---

## Next Steps

1. **Explore the Code**
   - Review `Program.cs` for DI setup
   - Check `ApplicationDbContext.cs` for database configuration
   - Study service implementations

2. **Make Changes**
   - Create new endpoints
   - Add business logic
   - Modify models

3. **Test Thoroughly**
   - Use Swagger for manual testing
   - Write unit tests
   - Load test the APIs

4. **Deploy**
   - Publish to Azure App Service
   - Configure for production
   - Set up CI/CD pipeline

---

## Support & Resources

- **Entity Framework Core**: https://docs.microsoft.com/ef/core/
- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core/
- **GitHub Repository**: https://github.com/karan321github/FormulaAirlineMicroService
- **Swagger/OpenAPI**: https://swagger.io/

---

**Happy Coding! 🚀**
