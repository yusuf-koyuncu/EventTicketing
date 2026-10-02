using EventTicketing.Core.DataAccess;
using EventTicketing.Core.Utilities.Results;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Entities;

namespace EventTicketing.DataAccess.Abstract;

public interface IEventDal : IEntityRepository<Event>
{
    Task<PagedResult<EventListItemDto>> GetListAsync(
        int page, int pageSize, bool includeDrafts, CancellationToken ct = default);

    Task<EventDetailDto?> GetDetailAsync(int eventId, CancellationToken ct = default);

    Task<Event?> GetWithSectionsAsync(int eventId, CancellationToken ct = default);

    Task<TicketPriceDto?> GetActivePriceAsync(
        int eventId, int seatCategoryId, CancellationToken ct = default);

    Task<List<VenueSection>> GetVenueSectionsWithSeatsAsync(
        int venueId, CancellationToken ct = default);

    void AddEventSection(EventSection eventSection);

    void AddEventSeat(EventSeat eventSeat);

    Task<AdminEventDto?> GetAdminDetailAsync(int eventId, CancellationToken ct = default);

    Task<List<SimpleListItemDto>> GetVenuesAsync(CancellationToken ct = default);

    Task<List<SimpleListItemDto>> GetOrganizersAsync(CancellationToken ct = default);

    Task<List<SimpleListItemDto>> GetSeatCategoriesAsync(int venueId, CancellationToken ct = default);

    Task SetPricesAsync(int eventId, List<PriceInput> prices, CancellationToken ct = default);
}
