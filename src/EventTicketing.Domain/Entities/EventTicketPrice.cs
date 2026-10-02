using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Entities;

public class EventTicketPrice : AuditableEntity
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int SeatCategoryId { get; set; }

    public decimal Price { get; set; }
    public decimal ServiceFee { get; set; }

    public bool IsActive { get; set; } = true;

    public Event Event { get; set; } = null!;
    public SeatCategory SeatCategory { get; set; } = null!;
}
