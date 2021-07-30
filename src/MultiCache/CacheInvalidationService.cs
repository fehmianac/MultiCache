using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MultiCache.Redis;

namespace MultiCache
{
    internal class CacheInvalidationService : BackgroundService
    {
        private readonly IRedisClient _redisClient;

        public CacheInvalidationService(IRedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _redisClient.SubscribeCacheInvalidationChannel(stoppingToken);
        }
    }
}
