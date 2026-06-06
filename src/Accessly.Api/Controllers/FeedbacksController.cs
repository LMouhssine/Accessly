using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Ai;
using Accessly.Application.Features.Feedbacks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api")]
public sealed class FeedbacksController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpPost("events/{eventId:guid}/feedbacks")]
    [Authorize]
    public async Task<ActionResult<FeedbackDto>> Submit(Guid eventId, [FromBody] SubmitFeedbackRequest body, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new SubmitFeedbackCommand(eventId, body.Rating, body.Comment), cancellationToken));

    [HttpGet("events/{eventId:guid}/feedbacks")]
    [Authorize(Roles = Roles.StaffOrAbove)]
    public async Task<ActionResult<FeedbackListDto>> List(Guid eventId, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetEventFeedbacksQuery(eventId), cancellationToken));

    [HttpPost("events/{eventId:guid}/feedbacks/ai-summary")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<ActionResult<AiTextResponse>> AiSummary(Guid eventId, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new SummarizeEventFeedbackCommand(eventId), cancellationToken));
}

public sealed record SubmitFeedbackRequest(int Rating, string? Comment);
