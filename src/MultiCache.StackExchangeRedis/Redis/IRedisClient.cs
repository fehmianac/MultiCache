using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCache.StackExchangeRedis.Redis
{
    public interface IRedisClient
    {
        Task<byte[]> GetAsync(string key, CancellationToken token = default);
        Task<(byte[], TimeSpan?)> GetWithExpiryAsync(string key, CancellationToken token = default);
        Task SetAsync(string key, byte[] value, TimeSpan? expiry = null, CancellationToken token = default);
        Task RemoveAsync(string key, CancellationToken token = default);
        Task SubscribeCacheInvalidationChannel(CancellationToken token = default);
    }
}