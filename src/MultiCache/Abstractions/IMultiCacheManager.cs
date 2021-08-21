using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCache.Abstractions
{
    public interface IMultiCacheManager
    {
        Task<(T, bool)> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken token = default);
        Task<(T, bool)> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken);
    }
}