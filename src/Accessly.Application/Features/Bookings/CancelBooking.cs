using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Bookings;

public sealed record CancelBookingCommand(Guid BookingId) : ICommand<Unit>;

public sealed class CancelBookingHandler(IAppDbContext db, ICurrentUser user, IAuditLogger audit)
    : IRequestHandler<CancelBookingCommand, Unit>
{
    public async Task<Unit> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        AccessGuard.EnsureAuthenticated(user);

        var booking = await db.Bookings
            .Include(b => b.Event)
            .Include(b => b.Ticket)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Booking), request.BookingId);

        if (booking.AttendeeUserId != user.UserId)
        {
            AccessGuard.EnsureCanManageOrganization(user, booking.Event.OrganizationId);
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ConflictException("This booking is already cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        if (booking.Ticket is not null && booking.Ticket.Status == TicketStatus.Active)
        {
            booking.Ticket.Status = TicketStatus.Cancelled;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.BookingCancelled, nameof(Booking), booking.Id.ToString(), null, cancellationToken);

        return Unit.Value;
    }
}
