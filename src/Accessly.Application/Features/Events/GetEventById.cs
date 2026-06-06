using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Events;

public sealed record GetEventByIdQuery(Guid Id) : IQuery<EventDetailDto>;

public sealed record GetEventBySlugQuery(string Slug) : IQuery<EventDetailDto>;

public sealed class GetEventByIdHandler(IAppDbContext db)
    : IRequestHandler<GetEventByIdQuery, EventDetailDto>
{
    public async Task<EventDetailDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await EventProjection.Project(db.Events.AsNoTracking().Where(e => e.Id == request.Id))
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Event), request.Id);
    }
}

public sealed class GetEventBySlugHandler(IAppDbContext db)
    : IRequestHandler<GetEventBySlugQuery, EventDetailDto>
{
    public async Task<EventDetailDto> Handle(GetEventBySlugQuery request, CancellationToken cancellationToken)
    {
        var dto = await EventProjection.Project(db.Events.AsNoTracking().Where(e => e.Slug == request.Slug))
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Event), request.Slug);
    }
}

internal static class EventProjection
{
    public static IQueryable<EventDetailDto> Project(IQueryable<Event> query) =>
        query.Select(e => new EventDetailDto(
            e.Id,
            e.Title,
            e.Slug,
            e.Description,
            e.Category,
            e.StartAt,
            e.EndAt,
            e.VenueName,
            e.VenueAddress,
            e.Capacity,
            e.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
            e.PriceAmount,
            e.Currency,
            e.Status,
            e.CoverImageUrl,
            e.OrganizationId,
            e.Speakers
                .OrderBy(s => s.Name)
                .Select(s => new SpeakerDto(s.Id, s.Name, s.Title, s.Bio))
                .ToList()));
}
