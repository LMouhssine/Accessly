using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Accessly.Infrastructure.Messaging;

/// <summary>In-process event bus used when no message broker is configured. Invokes handlers directly.</summary>
public sealed class InMemoryEventBus(IServiceProvider provider) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        foreach (var handler in provider.GetServices<IIntegrationEventHandler<TEvent>>())
        {
            await handler.HandleAsync(integrationEvent, cancellationToken);
        }
    }
}
