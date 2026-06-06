namespace Accessly.Application.Common.Messaging;

/// <summary>Marker for events published across process boundaries via the event bus.</summary>
public interface IIntegrationEvent;

/// <summary>Handles a published integration event.</summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
