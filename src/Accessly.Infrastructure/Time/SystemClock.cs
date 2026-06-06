using Accessly.Application.Common.Interfaces;

namespace Accessly.Infrastructure.Time;

/// <summary>Default <see cref="IClock"/> backed by the system clock.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
