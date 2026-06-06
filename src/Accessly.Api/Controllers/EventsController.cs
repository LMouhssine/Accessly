using Accessly.Application.Common.Messaging;
using Accessly.Application.Common.Models;
using Accessly.Application.Features.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api/events")]
public sealed class EventsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<EventSummaryDto>>> Get([FromQuery] GetEventsQuery query, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetEventByIdQuery(id), cancellationToken));

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetEventBySlugQuery(slug), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<ActionResult> Create([FromBody] CreateEventCommand command, CancellationToken cancellationToken)
    {
        var id = await Dispatcher.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventCommand command, CancellationToken cancellationToken)
    {
        await Dispatcher.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.Send(new PublishEventCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.Send(new UnpublishEventCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = Roles.OrganizerOrAdmin)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.Send(new CancelEventCommand(id), cancellationToken);
        return NoContent();
    }
}
