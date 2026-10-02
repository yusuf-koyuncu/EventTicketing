using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Dtos;

public class EventPriceDto
{
    public EventPriceDto(int seatCategoryId, string categoryName, decimal price, decimal serviceFee)
    {
        SeatCategoryId = seatCategoryId;
        CategoryName = categoryName;
        Price = price;
        ServiceFee = serviceFee;
    }

    public int SeatCategoryId { get; set; }
    public string CategoryName { get; set; }
    public decimal Price { get; set; }
    public decimal ServiceFee { get; set; }
}

public class EventDetailDto
{
    public EventDetailDto(
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
        DateTime cancellationCutoffUtc,
        int venueId,
        string venueName,
        string venueAddress,
        string venueCity,
        string venueCountry,
        string organizerName,
        List<EventPriceDto> prices)
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
        CancellationCutoffUtc = cancellationCutoffUtc;
        VenueId = venueId;
        VenueName = venueName;
        VenueAddress = venueAddress;
        VenueCity = venueCity;
        VenueCountry = venueCountry;
        OrganizerName = organizerName;
        Prices = prices;
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
    public DateTime CancellationCutoffUtc { get; set; }
    public int VenueId { get; set; }
    public string VenueName { get; set; }
    public string VenueAddress { get; set; }
    public string VenueCity { get; set; }
    public string VenueCountry { get; set; }
    public string OrganizerName { get; set; }
    public List<EventPriceDto> Prices { get; set; }
}
