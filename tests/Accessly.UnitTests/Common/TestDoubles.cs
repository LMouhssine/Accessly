using Accessly.Application.Common.Interfaces;
using Accessly.Application.Common.Messaging;
using Accessly.Domain.Enums;
using Accessly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Accessly.UnitTests.Common;

internal sealed class FakeCurrentUser(Guid? userId, UserRole? role, Guid? organizationId) : ICurrentUser
{
    public Guid? UserId => userId;
    public Guid? OrganizationId => organizationId;
    public UserRole? Role => role;
    public bool IsAuthenticated => userId is not null;
}

internal sealed class FixedClock(DateTimeOffset? now = null) : IClock
{
    public DateTimeOffset UtcNow { get; } = now ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}

internal sealed class NoOpAuditLogger : IAuditLogger
{
    public Task LogAsync(string action, string entityType, string? entityId = null, object? metadata = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class NoOpEventBus : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent => Task.CompletedTask;
}

/// <summary>A pass-through cache that always invokes the factory, so tests exercise the source.</summary>
internal sealed class PassThroughCache : ICacheService
{
    public Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan ttl, CancellationToken cancellationToken = default)
        => factory(cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal static class TestDb
{
    public static AppDbContext NewInMemory() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"accessly-tests-{Guid.NewGuid()}")
            .Options);
}
