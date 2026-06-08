using System.Text.Json;
using Accessly.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Accessly.Infrastructure.Caching;

/// <summary>
/// Read-through cache over <see cref="IDistributedCache"/>. A cache miss or a transient cache
/// fault falls back to the factory so a cache outage never breaks a request.
/// </summary>
public sealed class DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await cache.GetStringAsync(key, cancellationToken);
            if (cached is not null)
            {
                var value = JsonSerializer.Deserialize<T>(cached, SerializerOptions);
                if (value is not null)
                {
                    return value;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Cache read failed for key {CacheKey}; falling back to source.", key);
        }

        var fresh = await factory(cancellationToken);

        try
        {
            var serialized = JsonSerializer.Serialize(fresh, SerializerOptions);
            await cache.SetStringAsync(
                key,
                serialized,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Cache write failed for key {CacheKey}.", key);
        }

        return fresh;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Cache eviction failed for key {CacheKey}.", key);
        }
    }
}
