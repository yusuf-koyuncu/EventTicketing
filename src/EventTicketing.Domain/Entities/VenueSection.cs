using EventTicketing.Domain.Common;
using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Entities;

public class VenueSection : AuditableEntity
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public int SeatCategoryId { get; set; }
    public string Name { get; set; } = null!;
    public SectionType SectionType { get; set; }
    public int DisplayOrder { get; set; }
    public int? GeneralAdmissionCapacity { get; set; }

    public Venue Venue { get; set; } = null!;
    public SeatCategory SeatCategory { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<EventSection> EventSections { get; set; } = new List<EventSection>();
}
