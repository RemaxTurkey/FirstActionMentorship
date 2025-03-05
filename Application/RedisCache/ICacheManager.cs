namespace Application.RedisCache;

public interface ICacheManager
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync(string key, object data, int cacheTime);
    Task RemoveAsync(string key);
    Task<bool> IsSetAsync(string key);
}