using Accessly.Domain.Common;
using Accessly.Domain.Enums;

namespace Accessly.Domain.Entities;

/// <summary>A ticket issued for a confirmed booking, carrying a unique code and QR payload.</summary>
public class Ticket : AuditableEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    /// <summary>Denormalized for fast check-in lookups.</summary>
    public Guid EventId { get; set; }

    public string Code { get; set; } = string.Empty;
    public string QrPayload { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Active;
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}
