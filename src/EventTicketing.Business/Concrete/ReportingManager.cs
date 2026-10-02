using EventTicketing.Business.Abstract;
using EventTicketing.DataAccess.Abstract;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Exceptions;

namespace EventTicketing.Business.Concrete;

public class ReportingManager : IReportingService
{
    private readonly IReportingDal _reportingDal;

    public ReportingManager(IReportingDal reportingDal)
    {
        _reportingDal = reportingDal;
    }

    public Task<List<AvailableSeatDto>> GetAvailableSeatsAsync(
        int eventId, int? eventSectionId = null, CancellationToken ct = default) =>
        _reportingDal.GetAvailableSeatsAsync(eventId, eventSectionId, ct);

    public async Task<int> GetGeneralAdmissionRemainingAsync(
        int eventSectionId, CancellationToken ct = default) =>
        await _reportingDal.GetGeneralAdmissionRemainingAsync(eventSectionId, ct)
            ?? throw new EntityNotFoundException($"Event section {eventSectionId} does not exist.");

    public Task<List<SectionAvailabilityDto>> GetSectionAvailabilityAsync(
        int eventId, CancellationToken ct = default) =>
        _reportingDal.GetSectionAvailabilityAsync(eventId, ct);

    public async Task<EventOccupancyDto> GetOccupancyAsync(int eventId, CancellationToken ct = default) =>
        await _reportingDal.GetOccupancyAsync(eventId, ct)
            ?? throw new EntityNotFoundException($"Event {eventId} does not exist.");

    public Task<List<UserTicketDto>> GetUserTicketHistoryAsync(
        int userId, CancellationToken ct = default) =>
        _reportingDal.GetUserTicketHistoryAsync(userId, ct);

    public Task<decimal> GetRealisedRevenueAsync(int eventId, CancellationToken ct = default) =>
        _reportingDal.GetRealisedRevenueAsync(eventId, ct);
}
