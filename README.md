# MultiCache

[![CI](https://github.com/fehmianac/MultiCache/actions/workflows/tests.yml/badge.svg)](https://github.com/fehmianac/MultiCache/actions?query=workflow/tests)

## What is MultiCache?

MultiCache is a simple library designed to solve complex caching problems. In high-traffic applications, caching mechanisms are often distributed, which can introduce issues due to the use of TCP sockets for each cache request. MultiCache addresses this problem by caching objects both in the Cache Server (Redis) and in the server's memory. This allows subsequent requests to be served directly from memory, improving performance and reducing reliance on the Cache Server.

## Getting Started

To start using MultiCache, first configure it to specify the cache server in the startup of your application:

```csharp
services.AddRedisMultiCacheServices(options =>
{
    options.Configuration = "YOUR_REDIS_CONNECTION_STRING";
    options.InstanceName = "YOUR_REDIS_INSTANCE_NAME";
});
```

Then, in your application code, you can use the `IMultiCacheManager` interface, which provides the following methods:

```csharp
Task<(T, bool)> GetAsync<T>(string key, CancellationToken cancellationToken = default);
Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken token = default);
Task<(T, bool)> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
Task RemoveAsync(string key, CancellationToken cancellationToken);
```

## Installation

You can install the MultiCache.StackExchangeRedis package via NuGet Package Manager:

```
PM> Install-Package MultiCache.StackExchangeRedis
```

## Contributing

Contributions to MultiCache are welcome! If you encounter any issues or have suggestions for improvements, please feel free to create an issue or submit a pull request on the [GitHub repository](https://github.com/fehmianac/MultiCache). We appreciate your contributions to make MultiCache even better.

## License

MultiCache is released under the [MIT License](https://github.com/fehmianac/MultiCache/blob/main/LICENSE). Feel free to use, modify, and distribute it as per the terms of the license.
