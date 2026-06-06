using Accessly.Application.Common.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Accessly.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase(IDispatcher dispatcher) : ControllerBase
{
    protected IDispatcher Dispatcher { get; } = dispatcher;
}
