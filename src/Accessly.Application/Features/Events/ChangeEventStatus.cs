using Accessly.Application.Common;
using Accessly.Application.Common.Events;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Accessly.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Events;

public sealed record PublishEventCommand(Guid Id) : ICommand<Unit>;

public sealed record UnpublishEventCommand(Guid Id) : ICommand<Unit>;

public sealed record CancelEventCommand(Guid Id) : ICommand<Unit>;

public sealed class PublishEventHandler(IAppDbContext db, ICurrentUser user, IAuditLogger audit)
    : IRequestHandler<PublishEventCommand, Unit>
{
    public async Task<Unit> Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await EventStatusOps.LoadAndAuthorizeAsync(db, user, request.Id, cancellationToken);

        if (@event.Status != EventStatus.Draft)
        {
            throw new ConflictException("Only draft events can be published.");
        }

        @event.Status = EventStatus.Published;
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.EventPublished, nameof(Event), @event.Id.ToString(), null, cancellationToken);
        return Unit.Value;
    }
}

public sealed class UnpublishEventHandler(IAppDbContext db, ICurrentUser user, IAuditLogger audit)
    : IRequestHandler<UnpublishEventCommand, Unit>
{
    public async Task<Unit> Handle(UnpublishEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await EventStatusOps.LoadAndAuthorizeAsync(db, user, request.Id, cancellationToken);

        if (@event.Status != EventStatus.Published)
        {
            throw new ConflictException("Only published events can be unpublished.");
        }

        @event.Status = EventStatus.Draft;
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.EventUnpublished, nameof(Event), @event.Id.ToString(), null, cancellationToken);
        return Unit.Value;
    }
}

public sealed class CancelEventHandler(IAppDbContext db, ICurrentUser user, IAuditLogger audit, IEventBus eventBus)
    : IRequestHandler<CancelEventCommand, Unit>
{
    public async Task<Unit> Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await EventStatusOps.LoadAndAuthorizeAsync(db, user, request.Id, cancellationToken);

        if (@event.Status is EventStatus.Cancelled or EventStatus.Completed)
        {
            throw new ConflictException($"A {@event.Status.ToString().ToLowerInvariant()} event cannot be cancelled.");
        }

        @event.Status = EventStatus.Cancelled;
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync(AuditActions.EventCancelled, nameof(Event), @event.Id.ToString(), null, cancellationToken);
        await eventBus.PublishAsync(new EventCancelledIntegrationEvent(@event.Id), cancellationToken);
        return Unit.Value;
    }
}

internal static class EventStatusOps
{
    public static async Task<Event> LoadAndAuthorizeAsync(IAppDbContext db, ICurrentUser user, Guid id, CancellationToken cancellationToken)
    {
        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), id);
        AccessGuard.EnsureCanManageOrganization(user, @event.OrganizationId);
        return @event;
    }
}
