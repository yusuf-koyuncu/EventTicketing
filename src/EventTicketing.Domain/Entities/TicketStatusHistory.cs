using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Entities;

public class TicketStatusHistory
{
    private TicketStatusHistory() { }

    public long Id { get; private set; }
    public long TicketId { get; private set; }
    public TicketStatus? FromStatus { get; private set; }
    public TicketStatus ToStatus { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
    public string? Reason { get; private set; }
    public int? ChangedByUserId { get; private set; }

    public Ticket Ticket { get; private set; } = null!;
    public AppUser? ChangedByUser { get; private set; }

    public static TicketStatusHistory Create(
        TicketStatus? from, TicketStatus to, DateTime changedAtUtc, string? reason, int? changedByUserId) =>
        new()
        {
            FromStatus = from,
            ToStatus = to,
            ChangedAtUtc = changedAtUtc,
            Reason = reason,
            ChangedByUserId = changedByUserId
        };
}
