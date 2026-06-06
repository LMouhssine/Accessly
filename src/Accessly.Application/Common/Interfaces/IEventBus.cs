using Accessly.Application.Common.Messaging;

namespace Accessly.Application.Common.Interfaces;

/// <summary>Publishes integration events. Backed by RabbitMQ, or in-process when no broker is configured.</summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
