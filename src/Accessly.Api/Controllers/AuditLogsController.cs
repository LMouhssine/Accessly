using Accessly.Application.Common.Messaging;
using Accessly.Application.Common.Models;
using Accessly.Application.Features.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api/audit-logs")]
[Authorize(Roles = Roles.OrganizerOrAdmin)]
public sealed class AuditLogsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> Get([FromQuery] GetAuditLogsQuery query, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(query, cancellationToken));
}
