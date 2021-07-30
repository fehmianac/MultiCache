using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MultiCache.Redis
{
    internal class RedisCacheInvalidationService : BackgroundService
    {
        private readonly IRedisClient _redisClient;

        public RedisCacheInvalidationService(IRedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _redisClient.SubscribeCacheInvalidationChannel(stoppingToken);
        }
    }
}
