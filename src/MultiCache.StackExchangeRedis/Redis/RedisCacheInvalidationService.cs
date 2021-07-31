using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MultiCache.StackExchangeRedis.Redis
{
    public class RedisCacheInvalidationService : BackgroundService
    {
        private readonly IRedisClient _redisClient;

        public RedisCacheInvalidationService(IRedisClient redisClient)
        {
            _redisClient = redisClient;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _redisClient.SubscribeCacheInvalidationChannel(cancellationToken);
        }
    }
}
