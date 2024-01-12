namespace BlogApi.Services.Redis;

public interface IRedisService
{
    Task SetCacheResponseAsync(string cacheKey, string response, TimeSpan timeOut);
    Task<string> GetCacheResponseAsync(string cacheKey);
}
