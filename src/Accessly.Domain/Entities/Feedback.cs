using Accessly.Domain.Common;

namespace Accessly.Domain.Entities;

/// <summary>Attendee feedback left after an event (rating 1-5 plus an optional comment).</summary>
public class Feedback : AuditableEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid AttendeeUserId { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }
}
