namespace Accessly.Application.Common.Interfaces;

/// <summary>Abstraction over the system clock to keep handlers deterministic and testable.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
