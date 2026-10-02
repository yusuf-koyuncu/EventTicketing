using System.Text;
using EventTicketing.Business.Abstract;
using EventTicketing.Core.DataAccess;
using EventTicketing.Core.Utilities.Results;
using EventTicketing.DataAccess.Abstract;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Entities;
using EventTicketing.Domain.Enums;
using EventTicketing.Domain.Exceptions;

namespace EventTicketing.Business.Concrete;

public class EventManager : IEventService
{
    public const int MaxPageSize = 100;

    private readonly IEventDal _eventDal;
    private readonly IUnitOfWork _unitOfWork;

    public EventManager(IEventDal eventDal, IUnitOfWork unitOfWork)
    {
        _eventDal = eventDal;
        _unitOfWork = unitOfWork;
    }

    public Task<PagedResult<EventListItemDto>> GetListAsync(
        int page, int pageSize, bool includeDrafts, CancellationToken ct = default)
    {
        if (page < 1)
            throw new BusinessRuleException("Page must be 1 or greater.");

        if (pageSize is < 1 or > MaxPageSize)
            throw new BusinessRuleException($"Page size must be between 1 and {MaxPageSize}.");

        return _eventDal.GetListAsync(page, pageSize, includeDrafts, ct);
    }

    public async Task<EventDetailDto> GetDetailAsync(int eventId, CancellationToken ct = default) =>
        await _eventDal.GetDetailAsync(eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

    public async Task MaterializeInventoryAsync(int eventId, CancellationToken ct = default)
    {
        var @event = await _eventDal.GetWithSectionsAsync(eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

        if (@event.EventSections.Count > 0)
            return;

        var venueSections = await _eventDal.GetVenueSectionsWithSeatsAsync(@event.VenueId, ct);

        foreach (var venueSection in venueSections)
        {
            var eventSection = new EventSection
            {
                EventId = eventId,
                VenueSectionId = venueSection.Id,
                SeatCategoryId = venueSection.SeatCategoryId,
                GeneralAdmissionCapacity = venueSection.GeneralAdmissionCapacity,
                IsOnSale = true
            };
            _eventDal.AddEventSection(eventSection);

            if (venueSection.SectionType != SectionType.Reserved)
                continue;

            foreach (var seat in venueSection.Seats)
            {
                _eventDal.AddEventSeat(new EventSeat
                {
                    EventId = eventId,
                    EventSection = eventSection,
                    SeatId = seat.Id,
                    Status = EventSeatStatus.Available
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<AdminEventDto> GetAdminDetailAsync(int eventId, CancellationToken ct = default) =>
        await _eventDal.GetAdminDetailAsync(eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

    public Task<List<SimpleListItemDto>> GetVenuesAsync(CancellationToken ct = default) =>
        _eventDal.GetVenuesAsync(ct);

    public Task<List<SimpleListItemDto>> GetOrganizersAsync(CancellationToken ct = default) =>
        _eventDal.GetOrganizersAsync(ct);

    public Task<List<SimpleListItemDto>> GetSeatCategoriesAsync(
        int venueId, CancellationToken ct = default) =>
        _eventDal.GetSeatCategoriesAsync(venueId, ct);

    public async Task<int> CreateAsync(CreateEventRequest request, CancellationToken ct = default)
    {
        ValidateWindow(
            request.StartsAtUtc, request.EndsAtUtc,
            request.SalesStartUtc, request.SalesEndUtc,
            request.CancellationCutoffUtc);

        var @event = new Event
        {
            Name = request.Name.Trim(),
            Slug = await BuildUniqueSlugAsync(request.Name, ct),
            VenueId = request.VenueId,
            OrganizerId = request.OrganizerId,
            EventType = request.EventType,
            Status = EventStatus.Draft,
            Currency = request.Currency.ToUpperInvariant(),
            StartsAtUtc = request.StartsAtUtc,
            EndsAtUtc = request.EndsAtUtc,
            SalesStartUtc = request.SalesStartUtc,
            SalesEndUtc = request.SalesEndUtc,
            CancellationCutoffUtc = request.CancellationCutoffUtc
        };

        await _eventDal.AddAsync(@event, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return @event.Id;
    }

    public async Task UpdateAsync(int eventId, UpdateEventRequest request, CancellationToken ct = default)
    {
        ValidateWindow(
            request.StartsAtUtc, request.EndsAtUtc,
            request.SalesStartUtc, request.SalesEndUtc,
            request.CancellationCutoffUtc);

        var @event = await _eventDal.GetAsync(e => e.Id == eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

        @event.Name = request.Name.Trim();
        @event.EventType = request.EventType;
        @event.Status = request.Status;
        @event.StartsAtUtc = request.StartsAtUtc;
        @event.EndsAtUtc = request.EndsAtUtc;
        @event.SalesStartUtc = request.SalesStartUtc;
        @event.SalesEndUtc = request.SalesEndUtc;
        @event.CancellationCutoffUtc = request.CancellationCutoffUtc;

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SetPricesAsync(
        int eventId, SetPricesRequest request, CancellationToken ct = default)
    {
        if (await _eventDal.GetAsync(e => e.Id == eventId, ct) is null)
            throw new EntityNotFoundException($"Event {eventId} does not exist.");

        await _eventDal.SetPricesAsync(eventId, request.Prices, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task PublishAsync(int eventId, CancellationToken ct = default)
    {
        var @event = await _eventDal.GetAsync(e => e.Id == eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

        if (@event.Status == EventStatus.Draft)
            @event.Status = EventStatus.Published;

        await _unitOfWork.SaveChangesAsync(ct);
        await MaterializeInventoryAsync(eventId, ct);
    }

    public async Task SetPosterAsync(int eventId, string posterUrl, CancellationToken ct = default)
    {
        var @event = await _eventDal.GetAsync(e => e.Id == eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

        @event.PosterUrl = posterUrl;
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int eventId, CancellationToken ct = default)
    {
        var @event = await _eventDal.GetAsync(e => e.Id == eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

        if (@event.Status != EventStatus.Draft)
            throw new BusinessRuleException(
                "Only draft events can be deleted; a published event may already have tickets sold against it.");

        @event.IsDeleted = true;
        @event.DeletedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static void ValidateWindow(
        DateTime startsAtUtc, DateTime endsAtUtc,
        DateTime salesStartUtc, DateTime salesEndUtc,
        DateTime cancellationCutoffUtc)
    {
        if (endsAtUtc <= startsAtUtc)
            throw new BusinessRuleException("Event end must be after the start.");

        if (salesEndUtc <= salesStartUtc)
            throw new BusinessRuleException("Sales end must be after sales start.");

        if (cancellationCutoffUtc > startsAtUtc)
            throw new BusinessRuleException("Cancellation cutoff must be at or before the event start.");
    }

    private async Task<string> BuildUniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _eventDal.GetAsync(e => e.Slug == slug, ct) is not null)
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string Slugify(string name)
    {
        var builder = new StringBuilder();
        var previousWasDash = false;

        foreach (var ch in name.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                previousWasDash = false;
            }
            else if (!previousWasDash && builder.Length > 0)
            {
                builder.Append('-');
                previousWasDash = true;
            }
        }

        return builder.ToString().TrimEnd('-');
    }
}
