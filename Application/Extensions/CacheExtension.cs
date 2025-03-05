using Application.RedisCache;

namespace Application.Extensions;

public static class CacheExtension
{
    public static async Task<T> GetAsync<T>(this ICacheManager cacheManager,
        string key,
        int cacheTime,
        Func<Task<T>> acquire)
    {
        var tempKey = $"{key}";
        var flag = await cacheManager.IsSetAsync(tempKey);
        if (flag)
        {
            var async = await cacheManager.GetAsync<T>(tempKey);
            return async;
        }
        
        try
        {
            await cacheManager.SetAsync(tempKey, await acquire(), cacheTime);
        }
        catch
        {
            throw new Exception("Cache error.");
        }
        
        var obj = await acquire();
        await cacheManager.SetAsync(tempKey, obj, cacheTime);
        return obj;
    }
}