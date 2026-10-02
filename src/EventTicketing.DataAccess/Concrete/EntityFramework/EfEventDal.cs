using EventTicketing.Core.Utilities.Results;
using EventTicketing.DataAccess.Abstract;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.DataAccess.Concrete.EntityFramework;

public class EfEventDal : EfEntityRepositoryBase<Event, EventTicketingDbContext>, IEventDal
{
    public EfEventDal(EventTicketingDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<EventListItemDto>> GetListAsync(
        int page, int pageSize, bool includeDrafts, CancellationToken ct = default)
    {
        var query = Context.Events.AsNoTracking();

        if (!includeDrafts)
            query = query.Where(e => e.Status != EventStatus.Draft);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(e => e.StartsAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventListItemDto(
                e.Id,
                e.Name,
                e.Slug,
                e.EventType,
                e.Status,
                e.Currency,
                e.StartsAtUtc,
                e.EndsAtUtc,
                e.SalesStartUtc,
                e.SalesEndUtc,
                e.Venue.Name,
                e.Venue.City,
                e.Organizer.Name))
            .ToListAsync(ct);

        return new PagedResult<EventListItemDto>(items, page, pageSize, totalCount);
    }

    public async Task<EventDetailDto?> GetDetailAsync(int eventId, CancellationToken ct = default) =>
        await Context.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new EventDetailDto(
                e.Id,
                e.Name,
                e.Slug,
                e.EventType,
                e.Status,
                e.Currency,
                e.StartsAtUtc,
                e.EndsAtUtc,
                e.SalesStartUtc,
                e.SalesEndUtc,
                e.CancellationCutoffUtc,
                e.VenueId,
                e.Venue.Name,
                e.Venue.Address,
                e.Venue.City,
                e.Venue.Country,
                e.Organizer.Name,
                e.TicketPrices
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.SeatCategory.Name)
                    .Select(p => new EventPriceDto(
                        p.SeatCategoryId,
                        p.SeatCategory.Name,
                        p.Price,
                        p.ServiceFee))
                    .ToList()))
            .SingleOrDefaultAsync(ct);

    public async Task<Event?> GetWithSectionsAsync(int eventId, CancellationToken ct = default) =>
        await Context.Events
            .Include(e => e.EventSections)
            .SingleOrDefaultAsync(e => e.Id == eventId, ct);

    public async Task<TicketPriceDto?> GetActivePriceAsync(
        int eventId, int seatCategoryId, CancellationToken ct = default) =>
        await Context.EventTicketPrices
            .Where(p => p.EventId == eventId && p.SeatCategoryId == seatCategoryId && p.IsActive)
            .Select(p => new TicketPriceDto(p.Price, p.ServiceFee))
            .SingleOrDefaultAsync(ct);

    public async Task<List<VenueSection>> GetVenueSectionsWithSeatsAsync(
        int venueId, CancellationToken ct = default) =>
        await Context.VenueSections
            .Where(vs => vs.VenueId == venueId)
            .Include(vs => vs.Seats)
            .ToListAsync(ct);

    public void AddEventSection(EventSection eventSection) =>
        Context.EventSections.Add(eventSection);

    public void AddEventSeat(EventSeat eventSeat) =>
        Context.EventSeats.Add(eventSeat);

    public async Task<AdminEventDto?> GetAdminDetailAsync(int eventId, CancellationToken ct = default) =>
        await Context.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new AdminEventDto(
                e.Id,
                e.Name,
                e.VenueId,
                e.OrganizerId,
                e.EventType,
                e.Status,
                e.Currency,
                e.StartsAtUtc,
                e.EndsAtUtc,
                e.SalesStartUtc,
                e.SalesEndUtc,
                e.CancellationCutoffUtc,
                e.PosterUrl))
            .SingleOrDefaultAsync(ct);

    public async Task<List<SimpleListItemDto>> GetVenuesAsync(CancellationToken ct = default) =>
        await Context.Venues
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .Select(v => new SimpleListItemDto(v.Id, v.Name))
            .ToListAsync(ct);

    public async Task<List<SimpleListItemDto>> GetOrganizersAsync(CancellationToken ct = default) =>
        await Context.Organizers
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new SimpleListItemDto(o.Id, o.Name))
            .ToListAsync(ct);

    public async Task<List<SimpleListItemDto>> GetSeatCategoriesAsync(
        int venueId, CancellationToken ct = default) =>
        await Context.SeatCategories
            .AsNoTracking()
            .Where(c => c.VenueId == venueId)
            .OrderBy(c => c.Name)
            .Select(c => new SimpleListItemDto(c.Id, c.Name))
            .ToListAsync(ct);

    public async Task SetPricesAsync(
        int eventId, List<PriceInput> prices, CancellationToken ct = default)
    {
        var categoryIds = prices.Select(p => p.SeatCategoryId).ToList();

        var existing = await Context.EventTicketPrices
            .Where(p => p.EventId == eventId && categoryIds.Contains(p.SeatCategoryId))
            .ToDictionaryAsync(p => p.SeatCategoryId, ct);

        foreach (var input in prices)
        {
            if (existing.TryGetValue(input.SeatCategoryId, out var price))
            {
                price.Price = input.Price;
                price.ServiceFee = input.ServiceFee;
            }
            else
            {
                Context.EventTicketPrices.Add(new EventTicketPrice
                {
                    EventId = eventId,
                    SeatCategoryId = input.SeatCategoryId,
                    Price = input.Price,
                    ServiceFee = input.ServiceFee
                });
            }
        }
    }
}
