namespace Accessly.Application.Common.Messaging;

/// <summary>A request handled by exactly one handler, producing a response.</summary>
public interface IRequest<out TResponse>
{
}

/// <summary>A request that changes state.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>A request that reads state.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Represents the absence of a return value for commands.</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
