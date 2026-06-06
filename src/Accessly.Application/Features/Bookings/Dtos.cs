using Accessly.Domain.Enums;

namespace Accessly.Application.Features.Bookings;

public sealed record BookingDto(
    Guid Id,
    Guid EventId,
    string EventTitle,
    DateTimeOffset EventStartAt,
    Guid AttendeeUserId,
    string AttendeeName,
    BookingStatus Status,
    DateTimeOffset CreatedAt,
    Guid? TicketId,
    string? TicketCode,
    TicketStatus? TicketStatus);
