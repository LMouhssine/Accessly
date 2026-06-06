using Accessly.Domain.Enums;

namespace Accessly.Application.Features.Tickets;

public sealed record TicketDto(
    Guid Id,
    Guid BookingId,
    Guid EventId,
    string EventTitle,
    string Code,
    TicketStatus Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset? UsedAt,
    string AttendeeName);
