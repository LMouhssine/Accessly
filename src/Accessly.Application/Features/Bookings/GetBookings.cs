using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Application.Common.Models;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Bookings;

public sealed record GetBookingsQuery(
    Guid? EventId = null,
    Guid? AttendeeUserId = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<BookingDto>>;

public sealed class GetBookingsHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetBookingsQuery, PagedResult<BookingDto>>
{
    public async Task<PagedResult<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        AccessGuard.EnsureAuthenticated(user);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Bookings.AsNoTracking().AsQueryable();
        query = user.Role switch
        {
            UserRole.Admin => query,
            UserRole.Attendee => query.Where(b => b.AttendeeUserId == user.UserId),
            _ => query.Where(b => b.Event.OrganizationId == user.OrganizationId),
        };

        if (request.EventId.HasValue)
        {
            query = query.Where(b => b.EventId == request.EventId.Value);
        }

        if (request.AttendeeUserId.HasValue)
        {
            query = query.Where(b => b.AttendeeUserId == request.AttendeeUserId.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await BookingProjection.Project(query.OrderByDescending(b => b.CreatedAt))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BookingDto>(items, page, pageSize, total);
    }
}

public sealed record GetBookingByIdQuery(Guid Id) : IQuery<BookingDto>;

public sealed class GetBookingByIdHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        AccessGuard.EnsureAuthenticated(user);

        var booking = await db.Bookings.AsNoTracking()
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Booking), request.Id);

        if (booking.AttendeeUserId != user.UserId && user.Role != UserRole.Admin)
        {
            AccessGuard.EnsureCanManageOrganization(user, booking.Event.OrganizationId);
        }

        return await BookingProjection.Project(db.Bookings.AsNoTracking().Where(b => b.Id == request.Id))
            .FirstAsync(cancellationToken);
    }
}

internal static class BookingProjection
{
    public static IQueryable<BookingDto> Project(IQueryable<Booking> query) =>
        query.Select(b => new BookingDto(
            b.Id,
            b.EventId,
            b.Event.Title,
            b.Event.StartAt,
            b.AttendeeUserId,
            b.AttendeeUser.DisplayName,
            b.Status,
            b.CreatedAt,
            b.Ticket != null ? (Guid?)b.Ticket.Id : null,
            b.Ticket != null ? b.Ticket.Code : null,
            b.Ticket != null ? (TicketStatus?)b.Ticket.Status : null));
}
