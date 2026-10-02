using EventTicketing.Business.Concrete;
using EventTicketing.Core.Utilities.Security.Hashing;
using EventTicketing.DataAccess.Concrete.EntityFramework;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using EventTicketing.Tools.DbSeeder;
using Microsoft.EntityFrameworkCore;

EventTicketingDbContext db = new();

Console.WriteLine("Connecting to the database and applying migrations...");
db.Database.Migrate();
Console.WriteLine("Database is ready.");
Console.WriteLine();

var unitOfWork = new EfUnitOfWork(db);
var eventDal = new EfEventDal(db);
var ticketDal = new EfTicketDal(db);
var reportingDal = new EfReportingDal(db);

var events = new EventManager(eventDal, unitOfWork);
var tickets = new TicketManager(ticketDal, eventDal, unitOfWork);
var reporting = new ReportingManager(reportingDal);

var seeder = new DatabaseSeeder(db, events, tickets, new PasswordHashingHelper());
await seeder.SeedAsync();

var eventId = await db.Events.Select(e => e.Id).FirstAsync();

var occupancy = await reporting.GetOccupancyAsync(eventId);

Console.WriteLine("=== LIVE OCCUPANCY ===");
Console.WriteLine($"Event             : {occupancy.EventName}");
Console.WriteLine($"Sellable capacity : {occupancy.SellableCapacity}");
Console.WriteLine($"Sold tickets      : {occupancy.SoldTickets}");
Console.WriteLine($"Cancelled tickets : {occupancy.CancelledTickets}");
Console.WriteLine($"Available seats   : {occupancy.AvailableSeats}");
Console.WriteLine($"Occupancy rate    : {occupancy.OccupancyRate:P2}");

Console.WriteLine();
Console.WriteLine("=== TICKET TRACKING: who, when, which event, which category, how much, active or cancelled ===");

var buyerIds = await db.Tickets
    .Select(t => t.UserId)
    .Distinct()
    .ToListAsync();

foreach (var buyerId in buyerIds)
{
    var buyerName = await db.Users
        .Where(u => u.Id == buyerId)
        .Select(u => u.FullName)
        .SingleAsync();

    Console.WriteLine();
    Console.WriteLine($"[{buyerName}]");

    foreach (var ticket in await reporting.GetUserTicketHistoryAsync(buyerId))
    {
        Console.WriteLine(
            $"  {ticket.TicketNumber.ToString()[..8]} | {ticket.EventName} | {ticket.SeatLabel} " +
            $"| {ticket.CategoryName} | {ticket.PricePaid:0.00} + {ticket.ServiceFeePaid:0.00} {ticket.Currency} " +
            $"| bought {ticket.PurchasedAtUtc:yyyy-MM-dd HH:mm} | {ticket.Status}");

        foreach (var step in ticket.History)
        {
            var from = step.FromStatus?.ToString() ?? "-";
            Console.WriteLine($"      status trail: {from} -> {step.ToStatus} at {step.ChangedAtUtc:yyyy-MM-dd HH:mm} ({step.Reason})");
        }
    }
}

Console.WriteLine();
Console.WriteLine($"Realised revenue (cancelled excluded): {await reporting.GetRealisedRevenueAsync(eventId):0.00}");

await db.DisposeAsync();

if (System.Diagnostics.Debugger.IsAttached)
{
    Console.WriteLine();
    Console.WriteLine("Press any key to close...");
    Console.ReadKey();
}
