using EventTicketing.Domain.Common;
using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Entities;

public class Event : AuditableEntity
{
    public int Id { get; set; }
    public int VenueId { get; set; }
    public int OrganizerId { get; set; }

    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public EventType EventType { get; set; }
    public EventStatus Status { get; set; }
    public string Currency { get; set; } = null!;
    public string? PosterUrl { get; set; }

    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public DateTime SalesStartUtc { get; set; }
    public DateTime SalesEndUtc { get; set; }

    public DateTime CancellationCutoffUtc { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public Venue Venue { get; set; } = null!;
    public Organizer Organizer { get; set; } = null!;
    public ICollection<EventSection> EventSections { get; set; } = new List<EventSection>();
    public ICollection<EventTicketPrice> TicketPrices { get; set; } = new List<EventTicketPrice>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public bool IsOnSale(DateTime nowUtc) =>
        Status is EventStatus.Published or EventStatus.OnSale &&
        nowUtc >= SalesStartUtc && nowUtc <= SalesEndUtc;
}
