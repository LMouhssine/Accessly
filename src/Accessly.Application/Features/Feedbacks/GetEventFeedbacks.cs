using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Feedbacks;

public sealed record GetEventFeedbacksQuery(Guid EventId) : IQuery<FeedbackListDto>;

public sealed class GetEventFeedbacksHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetEventFeedbacksQuery, FeedbackListDto>
{
    public async Task<FeedbackListDto> Handle(GetEventFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);
        AccessGuard.EnsureCanOperateEvent(user, @event.OrganizationId);

        var items = await db.Feedbacks.AsNoTracking()
            .Where(f => f.EventId == request.EventId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackDto(f.Id, f.EventId, f.Rating, f.Comment, f.CreatedAt))
            .ToListAsync(cancellationToken);

        var average = items.Count > 0 ? Math.Round(items.Average(f => f.Rating), 2) : 0d;
        return new FeedbackListDto(request.EventId, items.Count, average, items);
    }
}
