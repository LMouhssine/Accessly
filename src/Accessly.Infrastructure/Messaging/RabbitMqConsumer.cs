using System.Text.Json;
using Accessly.Application.Common.Events;
using Accessly.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Accessly.Infrastructure.Messaging;

/// <summary>Consumes integration events from RabbitMQ and dispatches them to their handlers.</summary>
public sealed class RabbitMqConsumer(
    IServiceProvider provider,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
        };

        var connection = await factory.CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(_options.Queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(_options.Queue, _options.Exchange, "#", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                await DispatchAsync(eventArgs.RoutingKey, eventArgs.Body, stoppingToken);
                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed handling integration event {RoutingKey}", eventArgs.RoutingKey);
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(_options.Queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        logger.LogInformation("RabbitMQ consumer listening on queue {Queue}.", _options.Queue);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Shutting down.
        }

        await channel.DisposeAsync();
        await connection.DisposeAsync();
    }

    private async Task DispatchAsync(string routingKey, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        switch (routingKey)
        {
            case nameof(BookingConfirmedIntegrationEvent):
                await InvokeAsync(services, JsonSerializer.Deserialize<BookingConfirmedIntegrationEvent>(body.Span)!, cancellationToken);
                break;
            case nameof(EventCancelledIntegrationEvent):
                await InvokeAsync(services, JsonSerializer.Deserialize<EventCancelledIntegrationEvent>(body.Span)!, cancellationToken);
                break;
            default:
                logger.LogWarning("No handler mapping for routing key {RoutingKey}.", routingKey);
                break;
        }
    }

    private static async Task InvokeAsync<TEvent>(IServiceProvider services, TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        foreach (var handler in services.GetServices<IIntegrationEventHandler<TEvent>>())
        {
            await handler.HandleAsync(integrationEvent, cancellationToken);
        }
    }
}
