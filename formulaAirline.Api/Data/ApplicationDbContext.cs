using formulaAirline.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace formulaAirline.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Booking entity
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PassangerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassportNb).IsRequired().HasMaxLength(50);
                entity.Property(e => e.From).IsRequired().HasMaxLength(50);
                entity.Property(e => e.To).IsRequired().HasMaxLength(50);
                entity.Property(e => e.status).IsRequired();
                entity.Property(e => e.FlightId).IsRequired();
                entity.HasOne(e => e.Flight).WithMany(f => f.Bookings).HasForeignKey(e => e.FlightId);
            });

            // Configure Flight entity
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Departure).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Arrival).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Capacity).IsRequired();
                entity.Property(e => e.AvailableSeats).IsRequired();
                entity.Property(e => e.DepartureTime).IsRequired();
                entity.Property(e => e.ArrivalTime).IsRequired();
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");
            });

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BookingId).IsRequired();
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(50);
                entity.Property(e => e.PaymentDate).IsRequired();
                entity.HasOne(e => e.Booking).WithMany(b => b.Payments).HasForeignKey(e => e.BookingId);
            });
        }
    }
}
