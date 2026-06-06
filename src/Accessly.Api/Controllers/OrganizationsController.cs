using Accessly.Application.Common.Messaging;
using Accessly.Application.Features.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[Route("api/organizations")]
[Authorize(Roles = Roles.Admin)]
public sealed class OrganizationsController(IDispatcher dispatcher) : ApiControllerBase(dispatcher)
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrganizationDto>>> Get(CancellationToken cancellationToken)
        => Ok(await Dispatcher.Send(new GetOrganizationsQuery(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> Create([FromBody] CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organization = await Dispatcher.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), organization);
    }
}
