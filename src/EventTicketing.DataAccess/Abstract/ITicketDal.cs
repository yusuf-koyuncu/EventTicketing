using EventTicketing.Core.DataAccess;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Entities;

namespace EventTicketing.DataAccess.Abstract;

public interface ITicketDal : IEntityRepository<Ticket>
{
    Task<EventSeat?> LockEventSeatAsync(
        int eventId, long eventSeatId, CancellationToken ct = default);

    Task<EventSection?> LockEventSectionAsync(
        int eventId, int eventSectionId, CancellationToken ct = default);

    Task<bool> HasActiveTicketForSeatAsync(long eventSeatId, CancellationToken ct = default);

    Task<int> CountActiveTicketsInSectionAsync(int eventSectionId, CancellationToken ct = default);

    Task<SeatPurchaseInfoDto> GetSeatPurchaseInfoAsync(
        long eventSeatId, CancellationToken ct = default);

    Task<string> GetSectionNameAsync(int eventSectionId, CancellationToken ct = default);

    Task<Ticket?> GetWithEventAsync(long ticketId, CancellationToken ct = default);

    Task SaveNewTicketAsync(Ticket ticket, CancellationToken ct = default);
}
