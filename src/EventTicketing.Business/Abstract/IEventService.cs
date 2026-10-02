using EventTicketing.Core.Utilities.Results;
using EventTicketing.Domain.Dtos;

namespace EventTicketing.Business.Abstract;

public interface IEventService
{
    Task<PagedResult<EventListItemDto>> GetListAsync(
        int page, int pageSize, bool includeDrafts, CancellationToken ct = default);

    Task<EventDetailDto> GetDetailAsync(int eventId, CancellationToken ct = default);

    Task MaterializeInventoryAsync(int eventId, CancellationToken ct = default);

    Task<AdminEventDto> GetAdminDetailAsync(int eventId, CancellationToken ct = default);

    Task<List<SimpleListItemDto>> GetVenuesAsync(CancellationToken ct = default);

    Task<List<SimpleListItemDto>> GetOrganizersAsync(CancellationToken ct = default);

    Task<List<SimpleListItemDto>> GetSeatCategoriesAsync(int venueId, CancellationToken ct = default);

    Task<int> CreateAsync(CreateEventRequest request, CancellationToken ct = default);

    Task UpdateAsync(int eventId, UpdateEventRequest request, CancellationToken ct = default);

    Task SetPricesAsync(int eventId, SetPricesRequest request, CancellationToken ct = default);

    Task PublishAsync(int eventId, CancellationToken ct = default);

    Task SetPosterAsync(int eventId, string posterUrl, CancellationToken ct = default);

    Task DeleteAsync(int eventId, CancellationToken ct = default);
}
