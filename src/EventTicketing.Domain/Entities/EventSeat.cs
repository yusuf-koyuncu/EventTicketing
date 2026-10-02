using EventTicketing.Domain.Common;
using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Entities;

public class EventSeat : AuditableEntity
{
    public long Id { get; set; }
    public int EventId { get; set; }
    public int EventSectionId { get; set; }
    public long SeatId { get; set; }

    public EventSeatStatus Status { get; set; } = EventSeatStatus.Available;
    public string? BlockReason { get; set; }

    public Event Event { get; set; } = null!;
    public EventSection EventSection { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
