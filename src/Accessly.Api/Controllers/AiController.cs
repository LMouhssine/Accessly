using Accessly.Application.Features.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = Roles.OrganizerOrAdmin)]
public sealed class AiController(AiEventAssistantService assistant) : ControllerBase
{
    [HttpPost("event-description")]
    public async Task<ActionResult<AiTextResponse>> Description([FromBody] AiEventRequest request, CancellationToken cancellationToken)
        => Ok(await assistant.EventDescriptionAsync(request, cancellationToken));

    [HttpPost("event-tags")]
    public async Task<ActionResult<AiTagsResponse>> Tags([FromBody] AiEventRequest request, CancellationToken cancellationToken)
        => Ok(await assistant.SuggestTagsAsync(request, cancellationToken));

    [HttpPost("reminder-email")]
    public async Task<ActionResult<AiTextResponse>> ReminderEmail([FromBody] AiEventRequest request, CancellationToken cancellationToken)
        => Ok(await assistant.ReminderEmailAsync(request, cancellationToken));

    [HttpPost("event-agenda")]
    public async Task<ActionResult<AiTextResponse>> Agenda([FromBody] AiEventRequest request, CancellationToken cancellationToken)
        => Ok(await assistant.AgendaAsync(request, cancellationToken));

    [HttpPost("announcement")]
    public async Task<ActionResult<AiTextResponse>> Announcement([FromBody] AiEventRequest request, CancellationToken cancellationToken)
        => Ok(await assistant.AnnouncementAsync(request, cancellationToken));
}
