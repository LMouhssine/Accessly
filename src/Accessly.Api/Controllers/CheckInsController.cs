using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.CheckIns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api")]
[Authorize(Roles = Roles.StaffOrAbove)]
public sealed class CheckInsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpPost("check-ins")]
    public async Task<ActionResult<CheckInResponse>> Record([FromBody] RecordCheckInCommand command, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(command, cancellationToken));

    [HttpGet("events/{eventId:guid}/check-ins")]
    public async Task<ActionResult<IReadOnlyList<CheckInDto>>> List(Guid eventId, CancellationToken cancellationToken, [FromQuery] int take = 50)
        => Ok(await Dispatcher.Send(new GetEventCheckInsQuery(eventId, take), cancellationToken));

    [HttpGet("events/{eventId:guid}/check-ins/summary")]
    public async Task<ActionResult<CheckInSummaryDto>> Summary(Guid eventId, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetCheckInSummaryQuery(eventId), cancellationToken));
}
