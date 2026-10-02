namespace EventTicketing.Domain.Enums;

public enum EventType
{
    Concert = 1,
    Sports = 2,
    Theatre = 3,
    Conference = 4,
    Other = 5
}

public enum EventStatus
{
    Draft = 1,
    Published = 2,
    OnSale = 3,
    SoldOut = 4,
    Cancelled = 5,
    Completed = 6
}

public enum SectionType
{
    Reserved = 1,
    GeneralAdmission = 2
}

public enum EventSeatStatus
{
    Available = 1,
    Blocked = 2
}

public enum TicketStatus
{
    Active = 1,
    Cancelled = 2
}

public enum UserRole
{
    Customer = 1,
    Organizer = 2,
    Admin = 3
}
