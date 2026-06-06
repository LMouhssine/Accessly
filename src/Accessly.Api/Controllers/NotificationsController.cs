using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Get(CancellationToken cancellationToken, [FromQuery] int take = 50)
        => Ok(await Dispatcher.Send(new GetNotificationsQuery(take), cancellationToken));

    [HttpPost("test")]
    public async Task<ActionResult<NotificationDto>> Test([FromBody] SendTestNotificationCommand? command, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(command ?? new SendTestNotificationCommand(), cancellationToken));
}
