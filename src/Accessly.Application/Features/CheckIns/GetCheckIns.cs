using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.CheckIns;

public sealed record GetEventCheckInsQuery(Guid EventId, int Take = 50) : IQuery<IReadOnlyList<CheckInDto>>;

public sealed record GetCheckInSummaryQuery(Guid EventId) : IQuery<CheckInSummaryDto>;

public sealed class GetEventCheckInsHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetEventCheckInsQuery, IReadOnlyList<CheckInDto>>
{
    public async Task<IReadOnlyList<CheckInDto>> Handle(GetEventCheckInsQuery request, CancellationToken cancellationToken)
    {
        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);
        AccessGuard.EnsureCanOperateEvent(user, @event.OrganizationId);

        return await CheckInSummaryBuilder.RecentQuery(db, request.EventId, Math.Clamp(request.Take, 1, 200))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetCheckInSummaryHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetCheckInSummaryQuery, CheckInSummaryDto>
{
    public async Task<CheckInSummaryDto> Handle(GetCheckInSummaryQuery request, CancellationToken cancellationToken)
    {
        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);
        AccessGuard.EnsureCanOperateEvent(user, @event.OrganizationId);

        return await CheckInSummaryBuilder.BuildAsync(db, @event, 10, cancellationToken);
    }
}

internal static class CheckInSummaryBuilder
{
    public static IQueryable<CheckInDto> RecentQuery(IAppDbContext db, Guid eventId, int take) =>
        db.CheckIns.AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.CheckedInAt)
            .Take(take)
            .Select(c => new CheckInDto(
                c.Id,
                c.TicketId,
                c.Ticket.Code,
                c.Ticket.Booking.AttendeeUser.DisplayName,
                c.CheckedInAt,
                c.Result));

    public static async Task<CheckInSummaryDto> BuildAsync(IAppDbContext db, Event @event, int recentTake, CancellationToken cancellationToken)
    {
        var registered = await db.Bookings.CountAsync(b => b.EventId == @event.Id && b.Status == BookingStatus.Confirmed, cancellationToken);
        var checkedIn = await db.Tickets.CountAsync(t => t.EventId == @event.Id && t.Status == TicketStatus.Used, cancellationToken);
        var recent = await RecentQuery(db, @event.Id, recentTake).ToListAsync(cancellationToken);
        var fillRate = @event.Capacity > 0 ? Math.Round((double)checkedIn / @event.Capacity, 4) : 0d;

        return new CheckInSummaryDto(@event.Id, registered, checkedIn, @event.Capacity, fillRate, recent);
    }
}
