using Accessly.Application.Common;
using Accessly.Application.Common.Exceptions;
using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Ai;
using Accessly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accessly.Application.Features.Feedbacks;

public sealed record SummarizeEventFeedbackCommand(Guid EventId) : ICommand<AiTextResponse>;

public sealed class SummarizeEventFeedbackHandler(IAppDbContext db, ICurrentUser user, AiEventAssistantService assistant)
    : IRequestHandler<SummarizeEventFeedbackCommand, AiTextResponse>
{
    public async Task<AiTextResponse> Handle(SummarizeEventFeedbackCommand request, CancellationToken cancellationToken)
    {
        var @event = await db.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.EventId);
        AccessGuard.EnsureCanManageOrganization(user, @event.OrganizationId);

        var feedback = await db.Feedbacks.AsNoTracking()
            .Where(f => f.EventId == request.EventId)
            .Select(f => new FeedbackSnippet(f.Rating, f.Comment))
            .ToListAsync(cancellationToken);

        return await assistant.SummarizeFeedbackAsync(@event.Title, feedback, cancellationToken);
    }
}
