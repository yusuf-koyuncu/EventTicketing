using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Dtos;

public class AvailableSeatDto
{
    public AvailableSeatDto(
        long eventSeatId,
        long seatId,
        string sectionName,
        string categoryName,
        string? rowLabel,
        string seatNumber,
        decimal price,
        decimal serviceFee)
    {
        EventSeatId = eventSeatId;
        SeatId = seatId;
        SectionName = sectionName;
        CategoryName = categoryName;
        RowLabel = rowLabel;
        SeatNumber = seatNumber;
        Price = price;
        ServiceFee = serviceFee;
    }

    public long EventSeatId { get; set; }
    public long SeatId { get; set; }
    public string SectionName { get; set; }
    public string CategoryName { get; set; }
    public string? RowLabel { get; set; }
    public string SeatNumber { get; set; }
    public decimal Price { get; set; }
    public decimal ServiceFee { get; set; }
}

public class SectionAvailabilityDto
{
    public SectionAvailabilityDto(
        int eventSectionId,
        string sectionName,
        string categoryName,
        bool isGeneralAdmission,
        int capacity,
        int sold,
        int blocked)
    {
        EventSectionId = eventSectionId;
        SectionName = sectionName;
        CategoryName = categoryName;
        IsGeneralAdmission = isGeneralAdmission;
        Capacity = capacity;
        Sold = sold;
        Blocked = blocked;
    }

    public int EventSectionId { get; set; }
    public string SectionName { get; set; }
    public string CategoryName { get; set; }
    public bool IsGeneralAdmission { get; set; }
    public int Capacity { get; set; }
    public int Sold { get; set; }
    public int Blocked { get; set; }
}

public class EventOccupancyDto
{
    public EventOccupancyDto(
        int eventId,
        string eventName,
        int reservedCapacity,
        int generalAdmissionCapacity,
        int blockedSeats,
        int soldTickets,
        int cancelledTickets)
    {
        EventId = eventId;
        EventName = eventName;
        ReservedCapacity = reservedCapacity;
        GeneralAdmissionCapacity = generalAdmissionCapacity;
        BlockedSeats = blockedSeats;
        SoldTickets = soldTickets;
        CancelledTickets = cancelledTickets;
    }

    public int EventId { get; set; }
    public string EventName { get; set; }
    public int ReservedCapacity { get; set; }
    public int GeneralAdmissionCapacity { get; set; }
    public int BlockedSeats { get; set; }
    public int SoldTickets { get; set; }
    public int CancelledTickets { get; set; }

    public int SellableCapacity => ReservedCapacity + GeneralAdmissionCapacity;
    public int GrossCapacity => SellableCapacity + BlockedSeats;
    public int AvailableSeats => SellableCapacity - SoldTickets;
    public double OccupancyRate => SellableCapacity == 0 ? 0 : (double)SoldTickets / SellableCapacity;
}

public class TicketHistoryDto
{
    public TicketHistoryDto(TicketStatus? fromStatus, TicketStatus toStatus, DateTime changedAtUtc, string? reason)
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedAtUtc = changedAtUtc;
        Reason = reason;
    }

    public TicketStatus? FromStatus { get; set; }
    public TicketStatus ToStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? Reason { get; set; }
}

public class UserTicketDto
{
    public UserTicketDto(
        long ticketId,
        Guid ticketNumber,
        string eventName,
        DateTime eventStartsAtUtc,
        string venueName,
        string seatLabel,
        string categoryName,
        decimal pricePaid,
        decimal serviceFeePaid,
        string currency,
        DateTime purchasedAtUtc,
        TicketStatus status,
        List<TicketHistoryDto> history)
    {
        TicketId = ticketId;
        TicketNumber = ticketNumber;
        EventName = eventName;
        EventStartsAtUtc = eventStartsAtUtc;
        VenueName = venueName;
        SeatLabel = seatLabel;
        CategoryName = categoryName;
        PricePaid = pricePaid;
        ServiceFeePaid = serviceFeePaid;
        Currency = currency;
        PurchasedAtUtc = purchasedAtUtc;
        Status = status;
        History = history;
    }

    public long TicketId { get; set; }
    public Guid TicketNumber { get; set; }
    public string EventName { get; set; }
    public DateTime EventStartsAtUtc { get; set; }
    public string VenueName { get; set; }
    public string SeatLabel { get; set; }
    public string CategoryName { get; set; }
    public decimal PricePaid { get; set; }
    public decimal ServiceFeePaid { get; set; }
    public string Currency { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
    public TicketStatus Status { get; set; }
    public List<TicketHistoryDto> History { get; set; }
}
