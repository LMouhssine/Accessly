using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Tickets;

public sealed record GetTicketByIdQuery(Guid Id) : IQuery<TicketDto>;

public sealed class GetTicketByIdHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetTicketByIdQuery, TicketDto>
{
    public async Task<TicketDto> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets.AsNoTracking()
            .Include(t => t.Booking).ThenInclude(b => b.Event)
            .Include(t => t.Booking).ThenInclude(b => b.AttendeeUser)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.Id);

        TicketAccess.Ensure(user, ticket.Booking.AttendeeUserId, ticket.Booking.Event.OrganizationId);

        return new TicketDto(
            ticket.Id,
            ticket.BookingId,
            ticket.EventId,
            ticket.Booking.Event.Title,
            ticket.Code,
            ticket.Status,
            ticket.IssuedAt,
            ticket.UsedAt,
            ticket.Booking.AttendeeUser.DisplayName);
    }
}

internal static class TicketAccess
{
    public static void Ensure(ICurrentUser user, Guid attendeeId, Guid organizationId)
    {
        AccessGuard.EnsureAuthenticated(user);

        if (user.Role == UserRole.Admin || attendeeId == user.UserId)
        {
            return;
        }

        if (user.Role is UserRole.Organizer or UserRole.Staff && user.OrganizationId == organizationId)
        {
            return;
        }

        throw new ForbiddenAccessException();
    }
}
