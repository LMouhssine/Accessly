using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Tickets;

public sealed record GetTicketQrQuery(Guid Id) : IQuery<byte[]>;

public sealed class GetTicketQrHandler(IAppDbContext db, ICurrentUser user, IQrCodeGenerator qrCode)
    : IRequestHandler<GetTicketQrQuery, byte[]>
{
    public async Task<byte[]> Handle(GetTicketQrQuery request, CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets.AsNoTracking()
            .Include(t => t.Booking).ThenInclude(b => b.Event)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ticket), request.Id);

        TicketAccess.Ensure(user, ticket.Booking.AttendeeUserId, ticket.Booking.Event.OrganizationId);

        return qrCode.GeneratePng(ticket.QrPayload);
    }
}
