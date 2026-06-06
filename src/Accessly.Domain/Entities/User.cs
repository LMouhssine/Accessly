using Accessly.Domain.Common;
using Accessly.Domain.Enums;

namespace Accessly.Domain.Entities;

/// <summary>An account that authenticates against the platform.</summary>
public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Attendee;

    /// <summary>Organization an organizer or staff member belongs to. Null for global attendees/admins.</summary>
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
