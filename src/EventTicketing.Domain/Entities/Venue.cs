using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Entities;

public class Venue : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<SeatCategory> SeatCategories { get; set; } = new List<SeatCategory>();
    public ICollection<VenueSection> Sections { get; set; } = new List<VenueSection>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
