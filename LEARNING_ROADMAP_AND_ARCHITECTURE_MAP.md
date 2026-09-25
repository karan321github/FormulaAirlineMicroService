# 🗺️ Microservices Learning Roadmap & Architecture Map

---

## Learning Path Timeline

```
Month 1: Fundamentals
├── Week 1-2: SOLID Principles & Design Patterns
│   ├── Single Responsibility Principle
│   ├── Dependency Injection
│   ├── Repository Pattern
│   └── Factory Pattern
│
├── Week 3: API Design
│   ├── RESTful API principles
│   ├── HTTP Status Codes
│   ├── API Versioning
│   └── Documentation (Swagger)
│
└── Week 4: Database Design
	├── Entity-Relationship Design
	├── Indexing strategies
	├── Query optimization
	└── Migrations with EF Core

Month 2: Microservices Foundation
├── Week 5-6: Service Architecture
│   ├── Monolith to Microservices transition
│   ├── Service boundaries
│   ├── API Gateway pattern
│   └── Ocelot implementation
│
├── Week 7: Communication Patterns
│   ├── Synchronous (REST, gRPC)
│   ├── Asynchronous (Message buses)
│   ├── Event-driven architecture
│   └── Saga pattern
│
└── Week 8: Message Brokers
	├── RabbitMQ basics
	├── Topic & Queue concepts
	├── Pub-Sub pattern
	└── Dead letter handling

Month 3: Advanced Patterns
├── Week 9-10: Resilience & Fault Tolerance
│   ├── Circuit Breaker (Polly)
│   ├── Retry policies
│   ├── Bulkhead pattern
│   ├── Timeout management
│   └── Graceful degradation
│
├── Week 11: Testing
│   ├── Unit testing with Moq
│   ├── Integration testing
│   ├── Contract testing
│   └── Load testing
│
└── Week 12: DevOps & Deployment
	├── Docker containerization
	├── Docker Compose
	├── Kubernetes basics
	├── CI/CD pipelines
	└── Health checks & monitoring
```

---

## Current Project → Microservices Evolution

### Stage 1: Current Monolith (Week 0)
```
┌──────────────────────────────┐
│   FormulaAirline.Api         │
├──────────────────────────────┤
│ Controllers:                 │
│  - BookingController         │
│  - FlightController          │
│  - PaymentController         │
│                              │
│ Services:                    │
│  - FlightService             │
│  - PaymentService            │
│  - MessageProducer           │
│                              │
│ Database:                    │
│  - Single SQL Server DB      │
│                              │
│ Models:                      │
│  - Booking, Flight, Payment  │
└──────────────────────────────┘
		↓
Status: Fixed issue (circular references)
Next: Start decomposition
```

### Stage 2: Service Extraction (Week 3-4)
```
					↓
┌────────────────────────────────────────────┐
│         API Gateway (Ocelot)               │
│  Port: 5000                                │
└────────────────────────────────────────────┘
		↓                    ↓                    ↓
┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐
│ Flight Service   │  │ Booking Service  │  │Payment Service
│ Port: 5001       │  │ Port: 5002       │  │Port: 5003
├──────────────────┤  ├──────────────────┤  ├──────────────┤
│ Controllers      │  │ Controllers      │  │Controllers   │
│ Services         │  │ Services         │  │Services      │
│ Models           │  │ Models           │  │Models        │
│ DTOs             │  │ DTOs             │  │DTOs          │
│ Repositories     │  │ Repositories     │  │Repositories  │
│ Flight DB        │  │ Booking DB       │  │Payment DB    │
└──────────────────┘  └──────────────────┘  └──────────────┘
		↑                    ↑                    ↑
		│                    │                    │
		└────────────────────┼────────────────────┘
							 │
					┌────────▼──────────┐
					│  RabbitMQ (Local) │
					│  Message Bus      │
					├──────────────────┤
					│ Exchanges:       │
					│ - BookingEvents  │
					│ - PaymentEvents  │
					│ - FlightEvents   │
					└──────────────────┘
```

### Stage 3: Event-Driven Communication (Week 5-6)
```
┌─────────────────────────────────────────────────────────────────┐
│                        Message Bus (RabbitMQ)                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────┐          ┌──────────────────────┐   │
│  │ BookingEvents Exchange │         │ PaymentEvents Exch  │   │
│  ├──────────────────────┤          ├──────────────────────┤   │
│  │ - BookingCreated    │     ◄─────│ - PaymentCreated    │   │
│  │ - BookingConfirmed  │           │ - PaymentProcessed  │   │
│  │ - BookingCanceled   │──────┐    │ - PaymentFailed     │   │
│  └──────────────────────┘     │    └──────────────────────┘   │
│           ▲                   │               ▲                │
│           │                   │               │                │
│           │ Publish           │               │                │
│           │                   ▼               │ Subscribe      │
│       ┌───┴──────┐        ┌────────┐        │                │
│       │  Booking │        │Payment │        │                │
│       │ Service  │        │Service │        │                │
│       └──────────┘        └────────┘        │                │
│                                              │                │
│  ┌──────────────────────┐          ┌────────┴──────────┐    │
│  │ FlightEvents Exchange │         │Notification Exch  │    │
│  ├──────────────────────┤          ├──────────────────┤     │
│  │ - SeatsReserved     │           │ - EmailSent      │     │
│  │ - SeatReleased      │──────────┤ - SmsSent        │     │
│  └──────────────────────┘          │ - PushSent       │     │
│           ▲                         └──────────────────┘     │
│           │                              ▲                   │
│       Publish                        Subscribe               │
│           │                              │                   │
│       ┌───┴──────────┐           ┌──────┴────────────┐      │
│       │  Flight      │           │ Notification      │      │
│       │  Service     │           │ Service           │      │
│       └──────────────┘           └───────────────────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Stage 4: Advanced Infrastructure (Week 8-10)
```
┌─────────────────────────────────────────────────────────────────────┐
│                      Frontend (React/Angular)                       │
└────────────────────────────┬────────────────────────────────────────┘
							 │ HTTPS
┌────────────────────────────▼────────────────────────────────────────┐
│                    API Gateway (Ocelot)                             │
│  - Rate Limiting                                                    │
│  - Request Logging (Serilog)                                        │
│  - Load Balancing                                                   │
│  - Authentication/Authorization                                    │
└────┬─────────────────────────────────────────────────────────────┬──┘
	 │                                                              │
┌────▼────────────┐  ┌─────────────────┐  ┌────────────────────┐ │
│ Flight Service  │  │ Booking Service │  │ Payment Service    │ │
├────────────────┤  ├─────────────────┤  ├────────────────────┤ │
│ Resilience:    │  │ Resilience:     │  │ Resilience:        │ │
│ - Circuit Brk  │  │ - Circuit Brk   │  │ - Circuit Breaker  │ │
│ - Retry (3x)   │  │ - Retry (3x)    │  │ - Retry (3x)       │ │
│ - Timeout (30s)│  │ - Timeout (30s) │  │ - Timeout (30s)    │ │
│ - Caching      │  │ - Caching       │  │ - Caching          │ │
│ - Circuit: 5   │  │ - Circuit: 5    │  │ - Circuit: 5       │ │
│   failures     │  │   failures      │  │   failures         │ │
└────┬───────────┘  └────┬────────────┘  └────┬───────────────┘ │
	 │                   │                     │                 │
	 └─────────────┬─────┴─────────────────────┴─────────────────┘
				   │
		┌──────────▼──────────┐
		│  RabbitMQ/Kafka     │
		│  Message Bus        │
		│  - Dead Letters     │
		│  - Retries          │
		│  - DLQ handlers     │
		└──────────┬──────────┘
				   │
	┌──────────────┫──────────────┐
	│              │              │
┌───▼────────┐ ┌──▼────────┐ ┌───▼──────────┐
│  Flight DB │ │ Booking  │ │  Payment DB │
│  (SQL)     │ │  DB(SQL) │ │  (SQL)     │
└────────────┘ └──────────┘ └────────────┘

Backing Services:
┌────────────────────────────────────────────────────────┐
│              Supporting Infrastructure                 │
├────────────────────────────────────────────────────────┤
│ ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│ │ Redis Cache  │  │ Seq Logging  │  │ Prometheus   │ │
│ │ (Caching)    │  │ (Centralized │  │ (Metrics)    │ │
│ │              │  │  Logging)    │  │              │ │
│ └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                        │
│ ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│ │ Grafana      │  │ Jaeger       │  │ Consul       │ │
│ │ (Dashboard)  │  │ (Tracing)    │  │ (Discovery)  │ │
│ └──────────────┘  └──────────────┘  └──────────────┘ │
└────────────────────────────────────────────────────────┘
```

---

## Technology Stack Selection Guide

### Communication Protocols
```
Use Case                          Technology      Why
─────────────────────────────────────────────────────────────
Simple service calls              REST (HTTP)     Easy, well-known
High throughput, low latency      gRPC            Fast, binary
Guaranteed delivery               Message Bus     Async, reliable
Real-time updates                 SignalR         WebSocket-based
Background processing             Hangfire        Job scheduling
```

### Message Broker Comparison
```
Feature              RabbitMQ    Kafka       Azure Service Bus
───────────────────────────────────────────────────────────────
Ease of setup        ✅ Easy     ⚠️ Medium   ✅ Easy (Cloud)
Message ordering     ✅ Yes      ✅ Yes      ⚠️ Per partition
Replay messages      ❌ No       ✅ Yes      ✅ Yes
Throughput           ✅ High     🚀 Very     ✅ High
									High
Cost (self-hosted)   ✅ Free     ✅ Free     ❌ Paid
Learning curve       ✅ Easy     ⚠️ Medium   ⚠️ Medium
Community           ✅ Large    ✅ Large    ✅ Large
```

### Caching Strategies
```
Need                              Solution         Cost
──────────────────────────────────────────────────────────
High-frequency reads              Redis Cache      Medium
Distributed cache                 Redis            Medium
In-memory cache                   MemoryCache      Low
Browser caching                   HTTP Headers     Low
Database query cache              EF Core          Low
API response cache                Polly + Redis    Medium
```

---

## Service Interaction Patterns

### Pattern 1: Synchronous Request-Reply
```
User Request
	↓
Booking Service
	├─ HTTP GET → Flight Service ← No delays
	├─ HTTP GET → Payment Service ← No delays
	└─ Response
	↓
User receives response immediately
```
**Pros**: Simple, immediate feedback
**Cons**: Slower if services are slow, tight coupling

### Pattern 2: Asynchronous with Events
```
User Request
	↓
Booking Service
	├─ Saves booking
	├─ Publishes "BookingCreated" event
	└─ Returns immediately to user ✅
		 ↓
	 RabbitMQ
		 │
		 ├─ → Payment Service (processes async)
		 ├─ → Flight Service (processes async)
		 └─ → Notification Service (processes async)
			 ↓
		 Services process independently
		 ↓
		 Publish completion events
```
**Pros**: Fast responses, loose coupling, scalable
**Cons**: Complex to debug, eventual consistency

### Pattern 3: Hybrid (Best of Both)
```
User Request
	↓
Booking Service
	├─ Validate flight (sync HTTP) ← Fast operation
	├─ Check payment rules (sync HTTP) ← Fast operation
	├─ Save booking
	├─ Publish "BookingCreated" (async)
	└─ Return immediately ✅
		 ↓
	 Background jobs process asynchronously
	 ├─ Send email
	 ├─ Update inventory
	 └─ Process payment
```
**Pros**: Balanced approach, responsive + reliable
**Cons**: Moderate complexity

---

## Data Flow Examples

### Booking Creation Flow
```
1. Client POST /bookings
   {
	 "flightId": 1,
	 "passengerId": 100,
	 "passengerName": "John Doe"
   }

2. API Gateway routes to Booking Service (port 5002)

3. Booking Service:
   POST /api/bookings
   ├─ Validation ✅
   ├─ HTTP GET http://flight-service:5001/api/flights/1
   │   Response: Flight { availableSeats: 10, price: 299.99 }
   ├─ Create booking entity
   ├─ DB INSERT booking
   ├─ Publish BookingCreatedIntegrationEvent {
   │     bookingId: 42,
   │     flightId: 1,
   │     passengerId: 100,
   │     passengerName: "John Doe",
   │     price: 299.99
   │   }
   └─ Return 201 Created { bookingDto }

4. RabbitMQ distributes event to:

   ├─ Payment Service
   │  ├─ Receives event
   │  ├─ Creates payment record
   │  ├─ Sets status = Completed (free booking)
   │  ├─ DB INSERT payment
   │  ├─ Publish PaymentProcessedIntegrationEvent
   │  └─ Message ACK sent to RabbitMQ ✅
   │
   ├─ Notification Service
   │  ├─ Receives event
   │  ├─ Generate email content
   │  ├─ Send email to passenger
   │  ├─ Log notification
   │  └─ Message ACK ✅
   │
   └─ Flight Service
	  ├─ Receives event
	  ├─ Reserve seat (flightId: 1, qty: 1)
	  ├─ Update flight.availableSeats = 9
	  ├─ DB UPDATE flight
	  ├─ Publish FlightSeatsReservedIntegrationEvent
	  └─ Message ACK ✅

5. Booking Service (as event subscriber)
   ├─ Receives PaymentProcessedIntegrationEvent
   ├─ Update booking.status = Confirmed
   ├─ DB UPDATE booking
   ├─ Publish BookingConfirmedIntegrationEvent
   └─ Message ACK ✅

6. Everything complete!
   User can see booking in app
   Email is in passenger's inbox
   Flight has updated seat count
```

---

## Monitoring & Observability

### Metrics to Track
```
Application Metrics:
├─ Request latency (p50, p95, p99)
├─ Error rate
├─ Throughput (requests/sec)
├─ Service availability (uptime %)
├─ Cache hit rate
└─ Queue depth

Infrastructure Metrics:
├─ CPU usage
├─ Memory usage
├─ Disk space
├─ Network bandwidth
├─ Database connections
└─ File handles

Business Metrics:
├─ Bookings per hour
├─ Bookings per day
├─ Revenue per day
├─ Popular routes
└─ Peak hours
```

### Alerting Rules
```
HIGH PRIORITY (Page on-call immediately):
├─ Service down (no responses)
├─ Error rate > 5%
├─ Response time p99 > 5s
├─ Database down
└─ Message queue > 10,000 messages

MEDIUM PRIORITY (Create alert, review in morning):
├─ Error rate > 1%
├─ Response time p95 > 1s
├─ Cache hit rate < 80%
├─ Disk space < 20% remaining
└─ Database connections > 80% of pool

LOW PRIORITY (Informational):
├─ Deployed new version
├─ Migration completed
├─ Test successful
└─ Weekly report
```

---

## Next: Detailed Implementation Guide

Start with these in order:

**Week 1-2**:
- [ ] API Gateway (Ocelot) ← Start here
- [ ] Solution structure reorganization
- [ ] Extract Flight Service

**Week 3-4**:
- [ ] Create Booking Service
- [ ] Create Payment Service
- [ ] Implement RabbitMQ

**Week 5-6**:
- [ ] Event publishing
- [ ] Event subscribing
- [ ] Error handling

**Week 7-8**:
- [ ] Docker setup
- [ ] docker-compose.yml
- [ ] Local testing

**Week 9-12**:
- [ ] Kubernetes
- [ ] CI/CD pipeline
- [ ] Production deployment

---

**Status**: 📚 Visual guide complete - Ready to implement!
