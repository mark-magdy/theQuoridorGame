namespace QuoridorBackend.BLL.Services.Interfaces;

public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a cached value with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a cached value by key
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes multiple cached values by keys
    /// </summary>
    Task RemoveAsync(params string[] keys);

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Removes all cached values matching a pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
}
