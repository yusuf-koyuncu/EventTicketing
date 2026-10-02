using EventTicketing.DataAccess.Abstract;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.DataAccess.Concrete.EntityFramework;

public class EfReportingDal : IReportingDal
{
    private readonly EventTicketingDbContext _context;

    public EfReportingDal(EventTicketingDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableSeatDto>> GetAvailableSeatsAsync(
        int eventId, int? eventSectionId = null, CancellationToken ct = default)
    {
        var query = _context.EventSeats
            .AsNoTracking()
            .Where(es => es.EventId == eventId)
            .Where(es => es.EventSection.IsOnSale)
            .Where(es => es.Status == EventSeatStatus.Available)
            .Where(es => !es.Tickets.Any(t => t.Status == TicketStatus.Active));

        if (eventSectionId is not null)
            query = query.Where(es => es.EventSectionId == eventSectionId);

        return await query
            .OrderBy(es => es.EventSection.VenueSection.DisplayOrder)
            .ThenBy(es => es.Seat.RowLabel)
            .ThenBy(es => es.Seat.SeatNumber)
            .Select(es => new AvailableSeatDto(
                es.Id,
                es.SeatId,
                es.EventSection.VenueSection.Name,
                es.EventSection.SeatCategory.Name,
                es.Seat.RowLabel,
                es.Seat.SeatNumber,
                _context.EventTicketPrices
                    .Where(p => p.EventId == eventId && p.SeatCategoryId == es.EventSection.SeatCategoryId)
                    .Select(p => p.Price).FirstOrDefault(),
                _context.EventTicketPrices
                    .Where(p => p.EventId == eventId && p.SeatCategoryId == es.EventSection.SeatCategoryId)
                    .Select(p => p.ServiceFee).FirstOrDefault()))
            .ToListAsync(ct);
    }

    public async Task<int?> GetGeneralAdmissionRemainingAsync(
        int eventSectionId, CancellationToken ct = default)
    {
        var row = await _context.EventSections
            .AsNoTracking()
            .Where(s => s.Id == eventSectionId)
            .Select(s => new
            {
                Capacity = s.GeneralAdmissionCapacity ?? 0,
                Sold = s.Tickets.Count(t => t.Status == TicketStatus.Active)
            })
            .SingleOrDefaultAsync(ct);

        return row is null ? null : Math.Max(0, row.Capacity - row.Sold);
    }

    public async Task<List<SectionAvailabilityDto>> GetSectionAvailabilityAsync(
        int eventId, CancellationToken ct = default) =>
        await _context.EventSections
            .AsNoTracking()
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.VenueSection.DisplayOrder)
            .Select(s => new SectionAvailabilityDto(
                s.Id,
                s.VenueSection.Name,
                s.SeatCategory.Name,
                s.GeneralAdmissionCapacity != null,
                s.GeneralAdmissionCapacity ?? s.EventSeats.Count(es => es.Status != EventSeatStatus.Blocked),
                s.Tickets.Count(t => t.Status == TicketStatus.Active),
                s.EventSeats.Count(es => es.Status == EventSeatStatus.Blocked)))
            .ToListAsync(ct);

    public async Task<EventOccupancyDto?> GetOccupancyAsync(
        int eventId, CancellationToken ct = default) =>
        await _context.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new EventOccupancyDto(
                e.Id,
                e.Name,
                _context.EventSeats.Count(es => es.EventId == e.Id && es.Status != EventSeatStatus.Blocked),
                _context.EventSections.Where(s => s.EventId == e.Id)
                                      .Sum(s => s.GeneralAdmissionCapacity ?? 0),
                _context.EventSeats.Count(es => es.EventId == e.Id && es.Status == EventSeatStatus.Blocked),
                _context.Tickets.Count(t => t.EventId == e.Id && t.Status == TicketStatus.Active),
                _context.Tickets.Count(t => t.EventId == e.Id && t.Status == TicketStatus.Cancelled)))
            .SingleOrDefaultAsync(ct);

    public async Task<List<UserTicketDto>> GetUserTicketHistoryAsync(
        int userId, CancellationToken ct = default) =>
        await _context.Tickets
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.PurchasedAtUtc)
            .Select(t => new UserTicketDto(
                t.Id,
                t.TicketNumber,
                t.Event.Name,
                t.Event.StartsAtUtc,
                t.Event.Venue.Name,
                t.SeatLabelSnapshot,
                t.SeatCategory.Name,
                t.PricePaid,
                t.ServiceFeePaid,
                t.Currency,
                t.PurchasedAtUtc,
                t.Status,
                t.StatusHistory
                    .OrderBy(h => h.ChangedAtUtc)
                    .Select(h => new TicketHistoryDto(h.FromStatus, h.ToStatus, h.ChangedAtUtc, h.Reason))
                    .ToList()))
            .ToListAsync(ct);

    public async Task<decimal> GetRealisedRevenueAsync(int eventId, CancellationToken ct = default) =>
        await _context.Tickets
            .Where(t => t.EventId == eventId && t.Status == TicketStatus.Active)
            .SumAsync(t => t.PricePaid + t.ServiceFeePaid, ct);
}
