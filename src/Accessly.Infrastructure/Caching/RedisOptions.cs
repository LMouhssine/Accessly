namespace Accessly.Infrastructure.Caching;

/// <summary>Configuration for the distributed cache (Redis).</summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// When false (the default), an in-process distributed cache is used so the app runs
    /// without Redis. Set to true and provide <see cref="ConnectionString"/> to use Redis.
    /// </summary>
    public bool Enabled { get; set; }

    public string? Connection { get; set; }

    /// <summary>Prefix applied to every cache key to namespace this app's entries.</summary>
    public string InstanceName { get; set; } = "accessly:";
}
