# API Usage Examples

## Flight Management Examples

### Create a Flight
```http
POST /Flight HTTP/1.1
Content-Type: application/json

{
  "flightNumber": "FA101",
  "departure": "New York",
  "arrival": "London",
  "capacity": 180,
  "departureTime": "2024-12-25T10:00:00",
  "arrivalTime": "2024-12-25T22:00:00",
  "price": 750.00
}
```

### Search Flights
```http
GET /Flight/search?departure=New York&arrival=London HTTP/1.1
```

### Check Seat Availability
```http
GET /Flight/1/check-availability?seatsRequired=5 HTTP/1.1
```

### Get All Flights
```http
GET /Flight HTTP/1.1
```

### Update Flight
```http
PUT /Flight/1 HTTP/1.1
Content-Type: application/json

{
  "id": 1,
  "flightNumber": "FA101",
  "departure": "New York",
  "arrival": "London",
  "capacity": 200,
  "availableSeats": 150,
  "departureTime": "2024-12-25T10:00:00",
  "arrivalTime": "2024-12-25T22:00:00",
  "price": 800.00
}
```

---

## Booking Examples

### Create a Booking
```http
POST /Booking HTTP/1.1
Content-Type: application/json

{
  "pasangerName": "John Doe",
  "passportNb": "AB123456",
  "from": "New York",
  "to": "London",
  "flightId": 1
}
```

**Response**: 
```json
{
  "id": 1,
  "pasangerName": "John Doe",
  "passportNb": "AB123456",
  "from": "New York",
  "to": "London",
  "status": 1,
  "flightId": 1,
  "flight": null,
  "payments": []
}
```

### Get All Bookings
```http
GET /Booking HTTP/1.1
```

### Get Booking by ID
```http
GET /Booking/1 HTTP/1.1
```

### Search Bookings by Passenger Name
```http
GET /Booking/passenger/John Doe HTTP/1.1
```

### Update Booking
```http
PUT /Booking/1 HTTP/1.1
Content-Type: application/json

{
  "id": 1,
  "pasangerName": "John Doe",
  "passportNb": "AB123456",
  "from": "New York",
  "to": "London",
  "status": 1,
  "flightId": 1
}
```

### Cancel Booking
```http
DELETE /Booking/1 HTTP/1.1
```

---

## Payment Examples

### Create Payment Record
```http
POST /Payment HTTP/1.1
Content-Type: application/json

{
  "bookingId": 1,
  "amount": 750.00,
  "paymentMethod": "CreditCard"
}
```

### Process Payment
```http
POST /Payment/process HTTP/1.1
Content-Type: application/json

{
  "bookingId": 1,
  "amount": 750.00,
  "paymentMethod": "CreditCard"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Payment processed successfully"
}
```

### Get Payments for Booking
```http
GET /Payment/booking/1 HTTP/1.1
```

### Get Total Payment Amount for Booking
```http
GET /Payment/booking/1/total HTTP/1.1
```

**Response**:
```json
{
  "bookingId": 1,
  "totalAmount": 750.00
}
```

### Get All Payments
```http
GET /Payment HTTP/1.1
```

### Update Payment Status
```http
PUT /Payment/1 HTTP/1.1
Content-Type: application/json

{
  "id": 1,
  "bookingId": 1,
  "amount": 750.00,
  "status": "Completed",
  "paymentMethod": "CreditCard",
  "transactionId": "TXN-12345",
  "paymentDate": "2024-12-24T15:30:00"
}
```

### Delete Payment
```http
DELETE /Payment/1 HTTP/1.1
```

---

## Complete Booking Flow Example

### Step 1: Create a Flight
```bash
curl -X POST https://localhost:5001/Flight \
  -H "Content-Type: application/json" \
  -d '{
	"flightNumber": "FA101",
	"departure": "New York",
	"arrival": "London",
	"capacity": 180,
	"departureTime": "2024-12-25T10:00:00",
	"arrivalTime": "2024-12-25T22:00:00",
	"price": 750.00
  }'
```

### Step 2: Verify Flight Exists
```bash
curl -X GET https://localhost:5001/Flight/1 \
  -H "Accept: application/json"
```

### Step 3: Check Seat Availability
```bash
curl -X GET "https://localhost:5001/Flight/1/check-availability?seatsRequired=1" \
  -H "Accept: application/json"
```

### Step 4: Create Booking
```bash
curl -X POST https://localhost:5001/Booking \
  -H "Content-Type: application/json" \
  -d '{
	"pasangerName": "John Doe",
	"passportNb": "AB123456",
	"from": "New York",
	"to": "London",
	"flightId": 1
  }'
```

### Step 5: Process Payment
```bash
curl -X POST https://localhost:5001/Payment/process \
  -H "Content-Type: application/json" \
  -d '{
	"bookingId": 1,
	"amount": 750.00,
	"paymentMethod": "CreditCard"
  }'
```

### Step 6: Verify Total Payment
```bash
curl -X GET https://localhost:5001/Payment/booking/1/total \
  -H "Accept: application/json"
```

---

## Error Responses

### 400 Bad Request
```json
{
  "error": "No available seats on this flight"
}
```

### 404 Not Found
```json
{
  "error": "Booking with ID 999 not found"
}
```

### 500 Internal Server Error
```json
{
  "error": "Internal server error"
}
```

---

## Swagger UI

When running the application in Development mode, access the interactive API documentation:
- **URL**: https://localhost:5001/swagger
- All endpoints are documented with request/response schemas
- Can test endpoints directly from the UI

---

## Status Codes Reference

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 204 | No Content - Successful deletion/update |
| 400 | Bad Request - Invalid input |
| 404 | Not Found - Resource doesn't exist |
| 500 | Internal Server Error |

---

## Payment Status Values

- **Pending** - Payment awaiting processing
- **Completed** - Payment successfully processed
- **Failed** - Payment processing failed

## Booking Status Values

- **0** - Cancelled
- **1** - Confirmed
- **2** - Completed
- **3** - Refunded
