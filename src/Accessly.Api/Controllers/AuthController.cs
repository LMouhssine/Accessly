using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    /// <summary>Authenticates a user and returns a JWT access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(command, cancellationToken));
}
