using System;
using Microsoft.Extensions.Caching.Memory;

namespace MultiCache.Memory
{
    public class MemoryClient : IMemoryClient
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryClient(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool TryGetValue<T>(string key, out T response)
        {
            return _memoryCache.TryGetValue(key, out response);
        }

        public T Set<T>(string key, T value)
        {
            return _memoryCache.Set<T>(key, value);
        }

        public T Set<T>(string key, T value, TimeSpan expiry)
        {
            return _memoryCache.Set<T>(key, value, expiry);
        }
    }
}
