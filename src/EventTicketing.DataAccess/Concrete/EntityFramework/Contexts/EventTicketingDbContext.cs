using EventTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;

public class EventTicketingDbContext : DbContext
{
    public EventTicketingDbContext()
    {
    }

    public EventTicketingDbContext(DbContextOptions<EventTicketingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Venue> Venues { get; set; }
    public DbSet<SeatCategory> SeatCategories { get; set; }
    public DbSet<VenueSection> VenueSections { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Organizer> Organizers { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventSection> EventSections { get; set; }
    public DbSet<EventSeat> EventSeats { get; set; }
    public DbSet<EventTicketPrice> EventTicketPrices { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketStatusHistory> TicketStatusHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connectionString = Environment.GetEnvironmentVariable("EVENTTICKETING_CONNECTION")
            ?? "Server=.\\SQLEXPRESS;Database=EventTicketing;Trusted_Connection=True;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventTicketingDbContext).Assembly);
    }
}
