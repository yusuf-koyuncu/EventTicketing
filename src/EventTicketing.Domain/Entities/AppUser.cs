using EventTicketing.Domain.Common;
using EventTicketing.Domain.Enums;

namespace EventTicketing.Domain.Entities;

public class AppUser : AuditableEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Customer;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
