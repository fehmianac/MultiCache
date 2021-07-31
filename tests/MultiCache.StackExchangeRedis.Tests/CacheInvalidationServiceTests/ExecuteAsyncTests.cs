using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoFixture;
using Moq;
using MultiCache.StackExchangeRedis.Redis;
using Xunit;

namespace MultiCache.StackExchangeRedis.CacheInvalidationServiceTests
{
    public class ExecuteAsyncTests
    {
        private readonly Fixture _fixture;

        public ExecuteAsyncTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Should_Subscribe_Cache_Invalidation_Channel()
        {
            var mock = AutoMock.GetStrict();

            var redisClientMock = mock.Mock<IRedisClient>();
            redisClientMock.Setup(q => q.SubscribeCacheInvalidationChannel(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var cacheInvalidationService = mock.Create<RedisCacheInvalidationService>();
            await cacheInvalidationService.StartAsync(default);

            redisClientMock.Verify(q => q.SubscribeCacheInvalidationChannel(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}