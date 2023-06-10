
[![CI](https://github.com/fehmianac/MultiCache/actions/workflows/tests.yml/badge.svg)](https://github.com/fehmianac/Multicache/actions?query=workflow/tests)

### What is MultiCache?

MultiCache is simple little library built to solve a complex caching problem. High traffic applications use caching mechanism as distributed. That integration sometimes has problem issue. Because each cache request use TCP socket. Multicache library solves this problem. When application hit cache from CacheServer(Redis) the object is set also memory of the server. So other requests server can reply the requests from memory. 


### How do I get started?

First, configure MultiCache to know what cache serve will use, in the startup of your application:

```csharp
services.AddRedisMultiCacheServices(options =>
{
    options.Configuration = "YOUR_REDIS_CONNECTION_STRING";
    options.InstanceName = "YOUR_REDIS_INSTANCE_NAME";
});

```
Then in your application code has IMultiCacheManager Interface:

```csharp
Task<(T, bool)> GetAsync<T>(string key, CancellationToken cancellationToken = default);
Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken token = default);
Task<(T, bool)> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
Task RemoveAsync(string key, CancellationToken cancellationToken);
```

### Where can I get it?

```
PM> Install-Package MultiCache.StackExchangeRedis
```
