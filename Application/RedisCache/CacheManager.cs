using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.RedisCache;

public class CacheManager : ICacheManager
{
    private static Lazy<ConnectionMultiplexer> _lazyMultiplexer;
    private readonly IConfiguration _configuration;
    private readonly string EnvironmentName;
    private ConfigurationOptions _configurationOptions;
    
    public CacheManager(IConfiguration configuration)
    {
        _configuration = configuration;
        EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        InitiateConfig();
        _lazyMultiplexer = new(() => ConnectionMultiplexer.Connect(_configurationOptions));
    }
    
    private static ConnectionMultiplexer LazyConnection => _lazyMultiplexer.Value;
    private static IDatabase LazyDb => _lazyMultiplexer.Value.GetDatabase(10);
    
    
    public async Task<T> GetAsync<T>(string key)
    {
        if (!key.Contains($"{key}_{EnvironmentName}"))
        {
            key = $"{key}_{EnvironmentName}";
        }
        
        var redisValue = await LazyDb.StringGetAsync((RedisKey) key);
        
        if (redisValue.IsNullOrEmpty)
        {
            return default;
        }
        
        try 
        {
            return JsonSerializer.Deserialize<T>(redisValue.ToString(), JsonSerializerOptions());
        }
        catch (JsonException)
        {
            return default;
        }
    }
    
    public async Task SetAsync(string key, object data, int cacheTime)
    {
        var redisKey = $"{key}_{EnvironmentName}";
        await LazyDb.StringSetAsync((RedisKey) redisKey,
            (RedisValue) JsonSerializer.Serialize(data, JsonSerializerOptions()),
            TimeSpan.FromSeconds(cacheTime));
    }
    
    public async Task RemoveAsync(string key)
    {
        var tempKey = $"{key}_{EnvironmentName}";
        await LazyDb.KeyDeleteAsync((RedisKey) tempKey);
    }
    
    public async Task<bool> IsSetAsync(string key)
    {
        var tempKey = $"{key}_{EnvironmentName}";
        var flag = await LazyDb.KeyExistsAsync((RedisKey) tempKey);
        return flag;
    }
    
    public void BuildConfigOptions()
    {
        var redisConnectionString = _configuration.GetConnectionString("Redis");
        
        _configurationOptions = new()
        {
            EndPoints = { redisConnectionString },
            AllowAdmin = true,
            ConnectTimeout = 10000,
            SyncTimeout = 10000,
            AbortOnConnectFail = false
        };
    }
    
    private void InitiateConfig()
    {
        var redisConnectionString = _configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new Exception("Redis connection string is missing in configuration.");
        }
        
        BuildConfigOptions();
    }
    
    private JsonSerializerOptions JsonSerializerOptions()
    {
        return new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            MaxDepth = 64
        };
    }
}