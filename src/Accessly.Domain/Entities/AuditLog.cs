using Accessly.Domain.Common;

namespace Accessly.Domain.Entities;

/// <summary>An immutable record of a significant action taken in the system.</summary>
public class AuditLog : Entity
{
    public string Action { get; set; } = string.Empty;
    public Guid? ActorId { get; set; }
    public Guid? OrganizationId { get; set; }

    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Free-form contextual data serialized as JSON.</summary>
    public string? MetadataJson { get; set; }
}
