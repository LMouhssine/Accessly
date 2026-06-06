using Microsoft.Extensions.DependencyInjection;

namespace Accessly.Application.Common.Messaging;

/// <summary>
/// Lightweight in-process dispatcher. Resolves the handler for a request and wraps it with the
/// registered pipeline behaviors (outermost first), then invokes the pipeline.
/// </summary>
public sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handler = provider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for request '{requestType.Name}'.");
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;

        RequestHandlerDelegate<TResponse> pipeline = () =>
            (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviorMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!;
        var behaviors = provider.GetServices(behaviorType)
            .Where(b => b is not null)
            .Cast<object>()
            .Reverse()
            .ToArray();

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = () => (Task<TResponse>)behaviorMethod.Invoke(behavior, [request, next, cancellationToken])!;
        }

        return pipeline();
    }
}
