using QuoridorBackend.BLL.Services.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace QuoridorBackend.BLL.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _database.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch
        {
            // If deserialization fails, remove the corrupted cache entry
            await RemoveAsync(key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        var expirationTime = expiration ?? _defaultExpiration;
        
        await _database.StringSetAsync(key, serializedValue, expirationTime);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveAsync(params string[] keys)
    {
        if (keys.Length == 0)
            return;

        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        await _database.KeyDeleteAsync(redisKeys);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());
        
        var keys = server.Keys(pattern: pattern).ToArray();
        
        if (keys.Length > 0)
            await _database.KeyDeleteAsync(keys);
    }
}
