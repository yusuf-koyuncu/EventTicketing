using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Entities;

public class EventSection : AuditableEntity
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int VenueSectionId { get; set; }
    public int SeatCategoryId { get; set; }

    public int? GeneralAdmissionCapacity { get; set; }

    public bool IsOnSale { get; set; } = true;

    public Event Event { get; set; } = null!;
    public VenueSection VenueSection { get; set; } = null!;
    public SeatCategory SeatCategory { get; set; } = null!;
    public ICollection<EventSeat> EventSeats { get; set; } = new List<EventSeat>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
