using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api/tickets")]
[Authorize]
public sealed class TicketsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetTicketByIdQuery(id), cancellationToken));

    [HttpGet("{id:guid}/qr")]
    public async Task<IActionResult> GetQr(Guid id, CancellationToken cancellationToken)
    {
        var png = await Dispatcher.Send(new GetTicketQrQuery(id), cancellationToken);
        return File(png, "image/png");
    }
}
