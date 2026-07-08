using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace portfolio.DataAccess.Extensions;

public static class CacheExtensions
{
    public static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
    {
        var cacheValue = cache.GetString(key);
        if (string.IsNullOrEmpty(cacheValue))
        {
            value = default;
            return false;
        }

        value = JsonSerializer.Deserialize<T>(cacheValue);
        return true;
    }

    public static async Task StoreValue<T>(this IDistributedCache cache, string key, T value, TimeSpan? expirationFromNow = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationFromNow
        });
    }
}