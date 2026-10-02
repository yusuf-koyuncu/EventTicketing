using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Dtos;

public class EventListItemDto
{
    public EventListItemDto(
        int id,
        string name,
        string slug,
        EventType eventType,
        EventStatus status,
        string currency,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        DateTime salesStartUtc,
        DateTime salesEndUtc,
        string venueName,
        string venueCity,
        string organizerName)
    {
        Id = id;
        Name = name;
        Slug = slug;
        EventType = eventType;
        Status = status;
        Currency = currency;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        SalesStartUtc = salesStartUtc;
        SalesEndUtc = salesEndUtc;
        VenueName = venueName;
        VenueCity = venueCity;
        OrganizerName = organizerName;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public EventType EventType { get; set; }
    public EventStatus Status { get; set; }
    public string Currency { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public DateTime SalesStartUtc { get; set; }
    public DateTime SalesEndUtc { get; set; }
    public string VenueName { get; set; }
    public string VenueCity { get; set; }
    public string OrganizerName { get; set; }
}

public class TicketPriceDto
{
    public TicketPriceDto(decimal price, decimal serviceFee)
    {
        Price = price;
        ServiceFee = serviceFee;
    }

    public decimal Price { get; set; }
    public decimal ServiceFee { get; set; }
}

public class SeatPurchaseInfoDto
{
    public SeatPurchaseInfoDto(
        int eventSectionId,
        int seatCategoryId,
        string sectionName,
        bool isSectionOnSale,
        string? rowLabel,
        string seatNumber)
    {
        EventSectionId = eventSectionId;
        SeatCategoryId = seatCategoryId;
        SectionName = sectionName;
        IsSectionOnSale = isSectionOnSale;
        RowLabel = rowLabel;
        SeatNumber = seatNumber;
    }

    public int EventSectionId { get; set; }
    public int SeatCategoryId { get; set; }
    public string SectionName { get; set; }
    public bool IsSectionOnSale { get; set; }
    public string? RowLabel { get; set; }
    public string SeatNumber { get; set; }
}
