namespace Accessly.Application.Common.Messaging;

/// <summary>Delegate that invokes the next step in the request pipeline.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>A cross-cutting behavior wrapped around request handling.</summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
