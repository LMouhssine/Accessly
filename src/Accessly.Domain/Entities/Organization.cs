using Accessly.Domain.Common;

namespace Accessly.Domain.Entities;

/// <summary>A tenant that owns events. Data is scoped per organization.</summary>
public class Organization : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
