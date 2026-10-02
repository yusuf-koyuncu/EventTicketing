using EventTicketing.Core.Entities;

namespace EventTicketing.Domain.Common;

public abstract class AuditableEntity : IEntity
{
    public DateTime CreatedAtUtc { get; set; }
}
