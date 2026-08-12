using Microsoft.Extensions.Caching.Memory;

namespace SMAS.API.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (ttl.HasValue)
                options.SetAbsoluteExpiration(ttl.Value);
            else
                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
