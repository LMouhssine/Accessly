using Accessly.Application.Common.Messaging;
using Accessly.Application.Common.Models;
using Accessly.Application.Features.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api")]
[Authorize]
public sealed class BookingsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpPost("events/{eventId:guid}/bookings")]
    public async Task<ActionResult<BookingResult>> Create(Guid eventId, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new CreateBookingCommand(eventId), cancellationToken));

    [HttpGet("bookings")]
    public async Task<ActionResult<PagedResult<BookingDto>>> Get([FromQuery] GetBookingsQuery query, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(query, cancellationToken));

    [HttpGet("bookings/{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetBookingByIdQuery(id), cancellationToken));

    [HttpPost("bookings/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.Send(new CancelBookingCommand(id), cancellationToken);
        return NoContent();
    }
}
