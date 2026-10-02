using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Entities;

public class SeatCategory : AuditableEntity
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public string Name { get; set; } = null!;

    public Venue Venue { get; set; } = null!;
    public ICollection<VenueSection> VenueSections { get; set; } = new List<VenueSection>();
}
