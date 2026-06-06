namespace Accessly.Domain.Common;

/// <summary>Base type for all persisted entities, identified by a GUID.</summary>
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

/// <summary>Entity that tracks creation and last-update timestamps.</summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
