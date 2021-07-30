using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MultiCache.Redis
{
   public class RedisClient : IRedisClient, IDisposable
    {
        private readonly RedisClientOptions _options;
        private readonly IMemoryCache _memoryCache;
        private readonly string _instance;
        private volatile ConnectionMultiplexer _connection;
        private IDatabase _database;
        private bool _disposed;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisClient(IOptions<RedisClientOptions> optionsAccessor, IMemoryCache memoryCache)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;
            _instance = _options.InstanceName ?? string.Empty;
            _memoryCache = memoryCache;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token).ConfigureAwait(false);
            return await _database.StringGetAsync(_instance + key).ConfigureAwait(false);
        }

        public async Task<(byte[], TimeSpan?)> GetWithExpiryAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token).ConfigureAwait(false);

            var value = await _database.StringGetWithExpiryAsync(_instance + key).ConfigureAwait(false);
            return (value.Value, value.Expiry);
        }

        public async Task SetAsync(string key, byte[] value, TimeSpan? expiry = null, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token).ConfigureAwait(false);

            await _database.StringSetAsync(_instance + key, value, expiry).ConfigureAwait(false);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token).ConfigureAwait(false);

            await _database.KeyDeleteAsync(_instance + key).ConfigureAwait(false);
            await PublishToCacheInvalidationChannel(key, token).ConfigureAwait(false);
        }

        public async Task SubscribeCacheInvalidationChannel(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token).ConfigureAwait(false);
            await _connection.GetSubscriber().SubscribeAsync(_instance + "cic", (_, value) =>
            {
                _memoryCache.Remove(value.ToString());
            }).ConfigureAwait(false);
        }

        private async Task PublishToCacheInvalidationChannel(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token).ConfigureAwait(false);
            await _connection.GetSubscriber().PublishAsync(_instance + "cic", key).ConfigureAwait(false);
        }

        private async Task ConnectAsync(CancellationToken token = default)
        {
            CheckDisposed();
            token.ThrowIfCancellationRequested();

            if (_database != null)
            {
                return;
            }

            await _connectionLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (_database == null)
                {
                    if (_options.ConfigurationOptions != null)
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions).ConfigureAwait(false);
                    }
                    else
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration).ConfigureAwait(false);
                    }

                    _database = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _connection?.Close();
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
