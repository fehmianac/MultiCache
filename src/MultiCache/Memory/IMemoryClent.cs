using System;

namespace MultiCache.Memory
{
    public interface IMemoryClient
    {
        bool TryGetValue<T>(string key, out T response);

        T Set<T>(string key, T value);

        T Set<T>(string key, T value, TimeSpan expiry);
        
    }
}
