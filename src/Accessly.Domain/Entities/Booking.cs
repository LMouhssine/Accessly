using Accessly.Domain.Common;
using Accessly.Domain.Enums;

namespace Accessly.Domain.Entities;

/// <summary>An attendee's reservation of a place at an event.</summary>
public class Booking : AuditableEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid AttendeeUserId { get; set; }
    public User AttendeeUser { get; set; } = null!;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public Ticket? Ticket { get; set; }
    public Payment? Payment { get; set; }
}
