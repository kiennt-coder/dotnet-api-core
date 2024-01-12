using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using StackExchange.Redis;

namespace BlogApi.Services.Redis;

public class RedisService : IRedisService
{

    private readonly IDistributedCache? _distributedCache;
    private readonly IConnectionMultiplexer? _connectionMultiplexer;

    public RedisService(IDistributedCache distributedCache, IConnectionMultiplexer connectionMultiplexer)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<string> GetCacheResponseAsync(string cacheKey)
    {
        if (cacheKey is null || string.IsNullOrWhiteSpace(cacheKey))
        {
            Console.WriteLine("cacheKey: {0}", cacheKey);
            return null!;
        }

        if (_distributedCache is null)
        {
            Console.WriteLine("_idstributedCache is null!");
            return null!;
        }

        try
        {
            var cacheResponse = await _distributedCache.GetStringAsync(cacheKey);

            return string.IsNullOrEmpty(cacheResponse) ? null! : cacheResponse;
        }
        catch (Exception error)
        {
            Console.WriteLine("GetStringAsyncError:: {0}", error.ToJson());
            throw;
        }
    }

    public async Task SetCacheResponseAsync(string cacheKey, string response, TimeSpan timeOut)
    {
        if (response is null)
        {
            Console.WriteLine("response: {0}", response);
            return;
        }

        if (_distributedCache is null)
        {
            Console.WriteLine("_idstributedCache is null!");
            return;
        }

        try
        {
            await _distributedCache.SetStringAsync(cacheKey, response, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeOut
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("SetStringAsyncError:: {0}", error.ToJson());
            throw;
        }
    }
}
