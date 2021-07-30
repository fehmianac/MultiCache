using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MultiCache.Abstractions;
using MultiCache.Redis;

namespace MultiCache
{
    public class MultiCacheManager : IMultiCacheManager
    {
        private readonly IRedisClient _redisClient;
        private readonly IMemoryCache _memoryCache;
        
        public MultiCacheManager(IRedisClient redisClient, IMemoryCache memoryCache)
        {
            _redisClient = redisClient;
            _memoryCache = memoryCache;
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var isExistsInMemory = _memoryCache.TryGetValue(key, out T response);
            if (isExistsInMemory)
            {
                return response;
            }

            var (value, expiry) = await _redisClient.GetWithExpiryAsync(key, cancellationToken).ConfigureAwait(false);
            if (value == null)
            {
                return default;
            }

            response = JsonSerializer.Deserialize<T>(value);
            if (expiry.HasValue)
            {
                _memoryCache.Set(key, response, expiry.Value);
            }
            else
            {
                _memoryCache.Set(key, response);
            }

            return response;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken token = default)
        {
            if (expiry.HasValue)
            {
                _memoryCache.Set(key, value, expiry.Value);
            }
            else
            {
                _memoryCache.Set(key, value);
            }

            await _redisClient.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), expiry, token).ConfigureAwait(false);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var cacheResult = await GetAsync<T>(key, cancellationToken);
            if (cacheResult != null)
                return cacheResult;

            var result = await factory();

            if (result == null)
            {
                return default(T);
            }

            await SetAsync(key, result, expiry, cancellationToken);
            return result;
        }
    }
}
