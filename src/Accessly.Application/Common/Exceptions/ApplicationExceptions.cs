namespace Accessly.Application.Common.Exceptions;

/// <summary>Thrown when a requested resource does not exist.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.")
    {
    }
}

/// <summary>Thrown when the caller is not allowed to perform the requested action.</summary>
public sealed class ForbiddenAccessException(string message = "Access to this resource is denied.")
    : Exception(message);

/// <summary>Thrown when an operation conflicts with the current state (for example, a full event).</summary>
public sealed class ConflictException(string message) : Exception(message);
