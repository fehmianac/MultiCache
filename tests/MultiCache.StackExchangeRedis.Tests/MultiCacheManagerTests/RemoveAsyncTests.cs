using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using MultiCache.Memory;
using MultiCache.StackExchangeRedis.Redis;
using Xunit;

namespace MultiCache.StackExchangeRedis.MultiCacheManagerTests
{
    public class RemoveAsyncTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IRedisClient> _redisClientMock;
        private readonly AutoMock _autoMock;

        public RemoveAsyncTests()
        {
            _fixture = new Fixture();
            _autoMock = AutoMock.GetStrict();
            _redisClientMock = _autoMock.Mock<IRedisClient>();
        }

        [AutoData, Theory]
        public async Task Should_Throw_Exception_When_CancellationTokenRequested(string cacheKey)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => multiCacheManager.RemoveAsync(cacheKey, cancellationTokenSource.Token));
        }

        [AutoData, Theory]
        public async Task Should_Remove_Cache(string cacheKey)
        {
            _redisClientMock.Setup(q => q.RemoveAsync(cacheKey, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            await multiCacheManager.RemoveAsync(cacheKey, default);

            _redisClientMock.Verify(q => q.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}