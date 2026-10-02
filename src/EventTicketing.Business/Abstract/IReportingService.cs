using EventTicketing.Domain.Dtos;

namespace EventTicketing.Business.Abstract;

public interface IReportingService
{
    Task<List<AvailableSeatDto>> GetAvailableSeatsAsync(
        int eventId, int? eventSectionId = null, CancellationToken ct = default);

    Task<int> GetGeneralAdmissionRemainingAsync(int eventSectionId, CancellationToken ct = default);

    Task<List<SectionAvailabilityDto>> GetSectionAvailabilityAsync(
        int eventId, CancellationToken ct = default);

    Task<EventOccupancyDto> GetOccupancyAsync(int eventId, CancellationToken ct = default);

    Task<List<UserTicketDto>> GetUserTicketHistoryAsync(int userId, CancellationToken ct = default);

    Task<decimal> GetRealisedRevenueAsync(int eventId, CancellationToken ct = default);
}
