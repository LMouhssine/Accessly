namespace Accessly.Application.Common.Interfaces;

/// <summary>
/// A small read-through cache abstraction. Backed by Redis when configured and by an
/// in-process store otherwise, so the application runs without an external cache.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or invokes <paramref name="factory"/>,
    /// stores the result for <paramref name="ttl"/> and returns it.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a cached entry, if present.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
