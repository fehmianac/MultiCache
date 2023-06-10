using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MultiCache.Abstractions;
using MultiCache.Memory;

namespace MultiCache.StackExchangeRedis.Redis
{
    public class RedisMultiCacheManager : IMultiCacheManager
    {
        private readonly IRedisClient _redisClient;
        private readonly IMemoryClient _memoryCache;

        public RedisMultiCacheManager(IRedisClient redisClient, IMemoryClient memoryCache)
        {
            _redisClient = redisClient;
            _memoryCache = memoryCache;
        }

        public async Task<(T, bool)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var isExistsInMemory = _memoryCache.TryGetValue(key, out T response);
            if (isExistsInMemory)
            {
                return (response, true);
            }

            var (value, expiry) = await _redisClient.GetWithExpiryAsync(key, cancellationToken).ConfigureAwait(false);
            if (value == null)
            {
                return (default, false);
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

            return (response, false);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (expiry.HasValue)
            {
                _memoryCache.Set(key, value, expiry.Value);
            }
            else
            {
                _memoryCache.Set(key, value);
            }

            await _redisClient.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), expiry, cancellationToken).ConfigureAwait(false);
        }

        public async Task<(T, bool)> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (cacheResult, fromCache) = await GetAsync<T>(key, cancellationToken);
            if (cacheResult != null)
                return (cacheResult, fromCache);

            var result = await factory();

            if (result == null)
            {
                return (default(T), false);
            }

            await SetAsync(key, result, expiry, cancellationToken);
            return (result, true);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _redisClient.RemoveAsync(key, cancellationToken);
        }
    }
}