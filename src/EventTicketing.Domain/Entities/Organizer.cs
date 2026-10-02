using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Entities;

public class Organizer : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ContactEmail { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
