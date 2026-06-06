using Accessly.Domain.Common;
using Accessly.Domain.Enums;

namespace Accessly.Domain.Entities;

/// <summary>A notification message. Email delivery is simulated by default.</summary>
public class Notification : AuditableEntity
{
    public Guid? UserId { get; set; }
    public Guid? OrganizationId { get; set; }

    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.Email;

    public string? ToAddress { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTimeOffset? SentAt { get; set; }
}
