using EventTicketing.Domain.Common;
using EventTicketing.Domain.Enums;
using EventTicketing.Domain.Exceptions;

namespace EventTicketing.Domain.Entities;

public class Ticket : AuditableEntity
{
    private readonly List<TicketStatusHistory> _statusHistory = new();

    private Ticket() { }

    public long Id { get; private set; }

    public Guid TicketNumber { get; private set; }

    public int EventId { get; private set; }
    public int UserId { get; private set; }
    public int EventSectionId { get; private set; }

    public long? EventSeatId { get; private set; }

    public int SeatCategoryId { get; private set; }

    public string SeatLabelSnapshot { get; private set; } = null!;

    public decimal PricePaid { get; private set; }

    public decimal ServiceFeePaid { get; private set; }

    public string Currency { get; private set; } = null!;

    public DateTime PurchasedAtUtc { get; private set; }
    public TicketStatus Status { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    public Event Event { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;
    public EventSection EventSection { get; private set; } = null!;
    public EventSeat? EventSeat { get; private set; }
    public SeatCategory SeatCategory { get; private set; } = null!;

    public IReadOnlyCollection<TicketStatusHistory> StatusHistory => _statusHistory;

    public bool OccupiesInventory => Status == TicketStatus.Active;

    public decimal TotalPaid => PricePaid + ServiceFeePaid;

    public static Ticket Purchase(
        int eventId,
        int eventSectionId,
        long? eventSeatId,
        int seatCategoryId,
        int userId,
        decimal price,
        decimal serviceFee,
        string currency,
        string seatLabel,
        DateTime nowUtc,
        string? reason = null)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (serviceFee < 0) throw new ArgumentOutOfRangeException(nameof(serviceFee));

        var ticket = new Ticket
        {
            TicketNumber = Guid.NewGuid(),
            EventId = eventId,
            EventSectionId = eventSectionId,
            EventSeatId = eventSeatId,
            SeatCategoryId = seatCategoryId,
            UserId = userId,
            PricePaid = price,
            ServiceFeePaid = serviceFee,
            Currency = currency,
            SeatLabelSnapshot = seatLabel,
            PurchasedAtUtc = nowUtc,
            Status = TicketStatus.Active
        };

        ticket._statusHistory.Add(
            TicketStatusHistory.Create(null, TicketStatus.Active, nowUtc, reason ?? "Purchased", userId));

        return ticket;
    }

    public void Cancel(DateTime nowUtc, DateTime cancellationDeadlineUtc, string? reason, int? changedByUserId)
    {
        if (Status != TicketStatus.Active)
            throw new InvalidTicketTransitionException(
                $"Ticket {TicketNumber} is {Status} and cannot be cancelled.");

        if (nowUtc >= cancellationDeadlineUtc)
            throw new InvalidTicketTransitionException(
                $"Ticket {TicketNumber} can no longer be cancelled (deadline {cancellationDeadlineUtc:O}).");

        Transition(TicketStatus.Cancelled, nowUtc, reason, changedByUserId);
        CancelledAtUtc = nowUtc;
    }

    private void Transition(TicketStatus to, DateTime nowUtc, string? reason, int? changedByUserId)
    {
        var from = Status;
        Status = to;
        _statusHistory.Add(TicketStatusHistory.Create(from, to, nowUtc, reason, changedByUserId));
    }
}
