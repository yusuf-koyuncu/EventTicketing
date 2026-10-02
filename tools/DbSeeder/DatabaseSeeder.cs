using EventTicketing.Business.Abstract;
using EventTicketing.Core.Utilities.Security;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Tools.DbSeeder;

public class DatabaseSeeder
{
    public const string SeedPassword = "Passw0rd!";

    private readonly EventTicketingDbContext _db;
    private readonly IEventService _events;
    private readonly ITicketService _tickets;
    private readonly IPasswordHashingHelper _passwordHashing;

    public DatabaseSeeder(
        EventTicketingDbContext db,
        IEventService events,
        ITicketService tickets,
        IPasswordHashingHelper passwordHashing)
    {
        _db = db;
        _events = events;
        _tickets = tickets;
        _passwordHashing = passwordHashing;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Venues.AnyAsync(ct))
            return;

        var nowUtc = DateTime.UtcNow;

        var venue = new Venue
        {
            Name = "Nova Arena",
            Address = "1 Arena Way",
            City = "Istanbul",
            Country = "Turkey"
        };
        _db.Venues.Add(venue);

        var standard = new SeatCategory { Venue = venue, Name = "Standard" };
        var vip = new SeatCategory { Venue = venue, Name = "VIP" };
        var balcony = new SeatCategory { Venue = venue, Name = "Balcony" };
        var floor = new SeatCategory { Venue = venue, Name = "Floor" };
        _db.SeatCategories.AddRange(standard, vip, balcony, floor);

        var field = new VenueSection
        {
            Venue = venue,
            SeatCategory = floor,
            Name = "Field",
            SectionType = SectionType.GeneralAdmission,
            GeneralAdmissionCapacity = 2000,
            DisplayOrder = 0
        };
        _db.VenueSections.Add(field);

        var reservedSections = new[]
        {
            new VenueSection { Venue = venue, SeatCategory = standard, Name = "Block A", SectionType = SectionType.Reserved, DisplayOrder = 1 },
            new VenueSection { Venue = venue, SeatCategory = standard, Name = "Block B", SectionType = SectionType.Reserved, DisplayOrder = 2 },
            new VenueSection { Venue = venue, SeatCategory = vip, Name = "Block C - VIP", SectionType = SectionType.Reserved, DisplayOrder = 3 },
            new VenueSection { Venue = venue, SeatCategory = balcony, Name = "Balcony", SectionType = SectionType.Reserved, DisplayOrder = 4 }
        };
        _db.VenueSections.AddRange(reservedSections);

        var seatsPerSection = 292 / reservedSections.Length;
        foreach (var section in reservedSections)
        {
            for (var i = 1; i <= seatsPerSection; i++)
            {
                var row = (char)('A' + (i - 1) / 10);
                var number = ((i - 1) % 10) + 1;
                _db.Seats.Add(new Seat
                {
                    VenueSection = section,
                    RowLabel = row.ToString(),
                    SeatNumber = number.ToString()
                });
            }
        }

        var passwordHash = _passwordHashing.Hash(SeedPassword);

        var buyers = new[]
        {
            new AppUser { Email = "alice@example.com", FullName = "Alice Aydin", PasswordHash = passwordHash, Role = UserRole.Customer },
            new AppUser { Email = "bora@example.com", FullName = "Bora Kaya", PasswordHash = passwordHash, Role = UserRole.Customer },
            new AppUser { Email = "ceren@example.com", FullName = "Ceren Yildiz", PasswordHash = passwordHash, Role = UserRole.Customer }
        };
        _db.Users.AddRange(buyers);

        _db.Users.AddRange(
            new AppUser { Email = "admin@example.com", FullName = "Site Admin", PasswordHash = passwordHash, Role = UserRole.Admin },
            new AppUser { Email = "organizer@novalive.example", FullName = "Nova Live Organizer", PasswordHash = passwordHash, Role = UserRole.Organizer });

        var organizer = new Organizer { Name = "Nova Live Events", ContactEmail = "events@novalive.example" };
        _db.Organizers.Add(organizer);

        var concert = new Event
        {
            Venue = venue,
            Organizer = organizer,
            Name = "Midnight Skyline Tour",
            Slug = "midnight-skyline-tour",
            EventType = EventType.Concert,
            Status = EventStatus.OnSale,
            Currency = "TRY",
            StartsAtUtc = nowUtc.AddDays(45),
            EndsAtUtc = nowUtc.AddDays(45).AddHours(3),
            SalesStartUtc = nowUtc.AddDays(-7),
            SalesEndUtc = nowUtc.AddDays(45),
            CancellationCutoffUtc = nowUtc.AddDays(45)
        };
        _db.Events.Add(concert);

        await _db.SaveChangesAsync(ct);

        _db.EventTicketPrices.AddRange(
            new EventTicketPrice { Event = concert, SeatCategory = standard, Price = 750m, ServiceFee = 50m },
            new EventTicketPrice { Event = concert, SeatCategory = vip, Price = 1500m, ServiceFee = 100m },
            new EventTicketPrice { Event = concert, SeatCategory = balcony, Price = 550m, ServiceFee = 40m },
            new EventTicketPrice { Event = concert, SeatCategory = floor, Price = 400m, ServiceFee = 30m });

        await _db.SaveChangesAsync(ct);

        await _events.MaterializeInventoryAsync(concert.Id, ct);

        var rigged = await _db.EventSeats
            .Where(es => es.EventId == concert.Id && es.EventSection.VenueSection.Name == "Block A")
            .OrderBy(es => es.Id)
            .Take(2)
            .ToListAsync(ct);

        foreach (var seat in rigged)
        {
            seat.Status = EventSeatStatus.Blocked;
            seat.BlockReason = "Stage rigging";
        }
        await _db.SaveChangesAsync(ct);

        var gaSection = await _db.EventSections
            .SingleAsync(s => s.EventId == concert.Id && s.VenueSection.Name == "Field", ct);

        var reservedSeats = await _db.EventSeats
            .Where(es => es.EventId == concert.Id
                      && es.EventSection.VenueSection.Name == "Block B"
                      && es.Status == EventSeatStatus.Available)
            .OrderBy(es => es.Id)
            .Take(3)
            .ToListAsync(ct);

        var ticket1 = await _tickets.PurchaseReservedSeatAsync(concert.Id, reservedSeats[0].Id, buyers[0].Id, ct);
        await _tickets.PurchaseReservedSeatAsync(concert.Id, reservedSeats[1].Id, buyers[1].Id, ct);
        await _tickets.PurchaseReservedSeatAsync(concert.Id, reservedSeats[2].Id, buyers[2].Id, ct);

        await _tickets.PurchaseGeneralAdmissionAsync(concert.Id, gaSection.Id, buyers[0].Id, ct);
        await _tickets.PurchaseGeneralAdmissionAsync(concert.Id, gaSection.Id, buyers[1].Id, ct);

        await _tickets.CancelAsync(ticket1.Id, buyers[0].Id, "Changed plans", ct);
    }
}
