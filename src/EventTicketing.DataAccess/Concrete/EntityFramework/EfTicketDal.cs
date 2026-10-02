using EventTicketing.DataAccess.Abstract;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Enums;
using EventTicketing.Domain.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.DataAccess.Concrete.EntityFramework;

public class EfTicketDal : EfEntityRepositoryBase<Ticket, EventTicketingDbContext>, ITicketDal
{
    private const int SqlDuplicateKey = 2601;
    private const int SqlUniqueConstraint = 2627;

    public EfTicketDal(EventTicketingDbContext context) : base(context)
    {
    }

    public async Task<EventSeat?> LockEventSeatAsync(
        int eventId, long eventSeatId, CancellationToken ct = default) =>
        await Context.EventSeats
            .FromSqlInterpolated($@"
                SELECT * FROM [EventSeats] WITH (UPDLOCK, ROWLOCK)
                WHERE [Id] = {eventSeatId} AND [EventId] = {eventId}")
            .SingleOrDefaultAsync(ct);

    public async Task<EventSection?> LockEventSectionAsync(
        int eventId, int eventSectionId, CancellationToken ct = default) =>
        await Context.EventSections
            .FromSqlInterpolated($@"
                SELECT * FROM [EventSections] WITH (UPDLOCK, ROWLOCK)
                WHERE [Id] = {eventSectionId} AND [EventId] = {eventId}")
            .SingleOrDefaultAsync(ct);

    public async Task<bool> HasActiveTicketForSeatAsync(
        long eventSeatId, CancellationToken ct = default) =>
        await Context.Tickets
            .AnyAsync(t => t.EventSeatId == eventSeatId && t.Status == TicketStatus.Active, ct);

    public async Task<int> CountActiveTicketsInSectionAsync(
        int eventSectionId, CancellationToken ct = default) =>
        await Context.Tickets
            .CountAsync(t => t.EventSectionId == eventSectionId && t.Status == TicketStatus.Active, ct);

    public async Task<SeatPurchaseInfoDto> GetSeatPurchaseInfoAsync(
        long eventSeatId, CancellationToken ct = default) =>
        await Context.EventSeats
            .Where(es => es.Id == eventSeatId)
            .Select(es => new SeatPurchaseInfoDto(
                es.EventSectionId,
                es.EventSection.SeatCategoryId,
                es.EventSection.VenueSection.Name,
                es.EventSection.IsOnSale,
                es.Seat.RowLabel,
                es.Seat.SeatNumber))
            .SingleAsync(ct);

    public async Task<string> GetSectionNameAsync(
        int eventSectionId, CancellationToken ct = default) =>
        await Context.EventSections
            .Where(s => s.Id == eventSectionId)
            .Select(s => s.VenueSection.Name)
            .SingleAsync(ct);

    public async Task<Ticket?> GetWithEventAsync(long ticketId, CancellationToken ct = default) =>
        await Context.Tickets
            .Include(t => t.Event)
            .SingleOrDefaultAsync(t => t.Id == ticketId, ct);

    public async Task SaveNewTicketAsync(Ticket ticket, CancellationToken ct = default)
    {
        Context.Tickets.Add(ticket);

        try
        {
            await Context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new SeatUnavailableException(
                $"Seat {ticket.EventSeatId} was sold by another transaction.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql &&
        sql.Number is SqlDuplicateKey or SqlUniqueConstraint;
}
