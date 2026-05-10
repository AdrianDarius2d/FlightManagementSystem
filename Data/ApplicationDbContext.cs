using FlightManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlightManagementSystem.Data
{
    /// <summary>
    /// EF Core database context for the Flight Management System.
    /// Extends IdentityDbContext to include ASP.NET Core Identity tables.
    /// Satisfies SRS §5.4.24 (separation between business logic and data access).
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        /// <summary>Initialises a new instance of <see cref="ApplicationDbContext"/>.</summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── Core Domain Tables ────────────────────────────────────────────────
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // ── Notification System ───────────────────────────────────────────────
        public DbSet<Notification> Notifications { get; set; }

        // ── Audit, Logging and Administration Tables ──────────────────────────
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<BackupRecord> BackupRecords { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        // ── Legacy Tables (kept for backward compatibility) ───────────────────
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }

        /// <summary>Configures entity relationships and constraints.</summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Flight primary key is the flight number string
            builder.Entity<Flight>()
                .HasKey(f => f.FlightNumber);

            // Reservation → Flight (many-to-one)
            builder.Entity<Reservation>()
                .HasOne(r => r.Flight)
                .WithMany(f => f.Reservations)
                .HasForeignKey(r => r.FlightId);

            // Reservation → Passenger (many-to-one)
            builder.Entity<Reservation>()
                .HasOne(r => r.Passenger)
                .WithMany(p => p.Reservations)
                .HasForeignKey(r => r.PassengerId);

            // Payment → Reservation (one-to-one)
            builder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithOne(r => r.Payment)
                .HasForeignKey<Payment>(p => p.ReservationId);

            // Ticket → Reservation (one-to-one)
            builder.Entity<Ticket>()
                .HasOne(t => t.Reservation)
                .WithOne(r => r.Ticket)
                .HasForeignKey<Ticket>(t => t.ReservationId);

            // SystemSetting primary key is the Key string
            builder.Entity<SystemSetting>()
                .HasKey(s => s.Key);
        }
    }
}
