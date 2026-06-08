using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api/dashboard")]
[Authorize(Roles = Roles.StaffOrAbove)]
public sealed class DashboardController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary(CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetDashboardSummaryQuery(), cancellationToken));
}
