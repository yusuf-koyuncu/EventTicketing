using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Entities;

public class Seat : AuditableEntity
{
    public long Id { get; set; }
    public int VenueSectionId { get; set; }
    public string? RowLabel { get; set; }
    public string SeatNumber { get; set; } = null!;

    public VenueSection VenueSection { get; set; } = null!;
    public ICollection<EventSeat> EventSeats { get; set; } = new List<EventSeat>();
}
