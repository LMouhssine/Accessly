using Accessly.Domain.Common;
using Accessly.Domain.Enums;

namespace Accessly.Domain.Entities;

/// <summary>A record of a ticket validation attempt at an event entrance.</summary>
public class CheckIn : Entity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid EventId { get; set; }
    public Guid? StaffUserId { get; set; }

    public DateTimeOffset CheckedInAt { get; set; }
    public CheckInResult Result { get; set; }
}
