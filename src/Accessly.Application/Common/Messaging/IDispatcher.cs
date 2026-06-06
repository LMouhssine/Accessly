namespace Accessly.Application.Common.Messaging;

/// <summary>Routes a request to its handler, applying the configured pipeline behaviors.</summary>
public interface IDispatcher
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
