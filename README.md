# MultiCache

[![CI](https://github.com/fehmianac/MultiCache/actions/workflows/tests.yml/badge.svg)](https://github.com/fehmianac/MultiCache/actions?query=workflow/tests)

## Overview

MultiCache is a versatile library designed to address intricate caching challenges. In high-traffic applications, the distribution of caching mechanisms can lead to complications, particularly when relying on TCP sockets for each cache request. MultiCache tackles this issue by dual-caching objects, both in the Cache Server (Redis) and the server's memory. This approach optimizes subsequent requests by serving them directly from memory, enhancing performance and minimizing dependence on the Cache Server.

## Getting Started

To integrate MultiCache into your application, start by configuring it to specify the cache server during the startup phase:

```csharp
services.AddRedisMultiCacheServices(options =>
{
    options.Configuration = "YOUR_REDIS_CONNECTION_STRING";
    options.InstanceName = "YOUR_REDIS_INSTANCE_NAME";
});
```

Once configured, you can utilize the `IMultiCacheManager` interface in your application code, providing access to the following methods:

```csharp
Task<(T, bool)> GetAsync<T>(string key, CancellationToken cancellationToken = default);
Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken token = default);
Task<(T, bool)> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
Task RemoveAsync(string key, CancellationToken cancellationToken);
```

## Installation

You can install the MultiCache.StackExchangeRedis package via NuGet Package Manager:

```bash
dotnet add package MultiCache.StackExchangeRedis
```

## Cache Invalidation

When a cache is removed from Redis, the corresponding key is also removed from the memory across all working servers.

## Contributing

Contributions to MultiCache are highly appreciated! If you encounter any issues or have suggestions for improvements, please feel free to create an issue or submit a pull request on the [GitHub repository](https://github.com/fehmianac/MultiCache). Your contributions play a vital role in enhancing MultiCache.

## License

MultiCache is released under the [MIT License](https://github.com/fehmianac/MultiCache/blob/main/LICENSE). Feel free to use, modify, and distribute it in accordance with the terms of the license.
