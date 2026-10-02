using System.ComponentModel.DataAnnotations;
using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Dtos;

public class SimpleListItemDto
{
    public SimpleListItemDto(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}

public class CreateEventRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int VenueId { get; set; }

    [Range(1, int.MaxValue)]
    public int OrganizerId { get; set; }

    public EventType EventType { get; set; }

    [Required, MaxLength(3), MinLength(3)]
    public string Currency { get; set; } = "TRY";

    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public DateTime SalesStartUtc { get; set; }
    public DateTime SalesEndUtc { get; set; }
    public DateTime CancellationCutoffUtc { get; set; }
}

public class UpdateEventRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    public EventType EventType { get; set; }
    public EventStatus Status { get; set; }

    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public DateTime SalesStartUtc { get; set; }
    public DateTime SalesEndUtc { get; set; }
    public DateTime CancellationCutoffUtc { get; set; }
}

public class PriceInput
{
    [Range(1, int.MaxValue)]
    public int SeatCategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ServiceFee { get; set; }
}

public class SetPricesRequest
{
    [Required, MinLength(1)]
    public List<PriceInput> Prices { get; set; } = new();
}

public class AdminEventDto
{
    public AdminEventDto(
        int id,
        string name,
        int venueId,
        int organizerId,
        EventType eventType,
        EventStatus status,
        string currency,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        DateTime salesStartUtc,
        DateTime salesEndUtc,
        DateTime cancellationCutoffUtc,
        string? posterUrl)
    {
        Id = id;
        Name = name;
        VenueId = venueId;
        OrganizerId = organizerId;
        EventType = eventType;
        Status = status;
        Currency = currency;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        SalesStartUtc = salesStartUtc;
        SalesEndUtc = salesEndUtc;
        CancellationCutoffUtc = cancellationCutoffUtc;
        PosterUrl = posterUrl;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int VenueId { get; set; }
    public int OrganizerId { get; set; }
    public EventType EventType { get; set; }
    public EventStatus Status { get; set; }
    public string Currency { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public DateTime SalesStartUtc { get; set; }
    public DateTime SalesEndUtc { get; set; }
    public DateTime CancellationCutoffUtc { get; set; }
    public string? PosterUrl { get; set; }
}
