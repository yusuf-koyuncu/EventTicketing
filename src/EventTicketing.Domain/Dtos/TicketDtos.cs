using System.ComponentModel.DataAnnotations;
using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Dtos;

public class PurchaseReservedSeatRequest
{
    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    [Range(1, long.MaxValue)]
    public long EventSeatId { get; set; }
}

public class PurchaseGeneralAdmissionRequest
{
    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    [Range(1, int.MaxValue)]
    public int EventSectionId { get; set; }
}

public class CancelTicketRequest
{
    [MaxLength(400)]
    public string? Reason { get; set; }
}

public class TicketDto
{
    public long TicketId { get; set; }
    public Guid TicketNumber { get; set; }
    public int EventId { get; set; }
    public int EventSectionId { get; set; }
    public long? EventSeatId { get; set; }
    public int SeatCategoryId { get; set; }
    public string SeatLabelSnapshot { get; set; } = null!;
    public decimal PricePaid { get; set; }
    public decimal ServiceFeePaid { get; set; }
    public decimal TotalPaid { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime PurchasedAtUtc { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
}
