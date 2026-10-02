using EventTicketing.Domain.Entities;

namespace EventTicketing.Business.Abstract;

public interface ITicketService
{
    Task<Ticket> PurchaseReservedSeatAsync(
        int eventId, long eventSeatId, int userId, CancellationToken ct = default);

    Task<Ticket> PurchaseGeneralAdmissionAsync(
        int eventId, int eventSectionId, int userId, CancellationToken ct = default);

    Task CancelAsync(long ticketId, int userId, string? reason, CancellationToken ct = default);
}
