using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MultiCache.StackExchangeRedis.Redis
{
    public class RedisCacheInvalidationService : IHostedService
    {
        private readonly IRedisClient _redisClient;

        public RedisCacheInvalidationService(IRedisClient redisClient)
        {
            _redisClient = redisClient;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _redisClient.SubscribeCacheInvalidationChannel(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
