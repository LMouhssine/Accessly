using Accessly.Domain.Common;

namespace Accessly.Domain.Entities;

/// <summary>A speaker or presenter associated with an event.</summary>
public class Speaker : Entity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Bio { get; set; }
}
