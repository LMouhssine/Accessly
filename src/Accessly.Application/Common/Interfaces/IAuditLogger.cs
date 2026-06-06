namespace Accessly.Application.Common.Interfaces;

/// <summary>Records significant actions to the audit log.</summary>
public interface IAuditLogger
{
    Task LogAsync(string action, string entityType, string? entityId = null, object? metadata = null, CancellationToken cancellationToken = default);
}
