using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using MultiCache.Memory;
using MultiCache.Redis;
using Xunit;

namespace MultiCache.Tests.MultiCacheManagerTests
{
    public class SetAsyncTests
    {
        private readonly Fixture _fixture;

        private readonly Mock<IMemoryClient> _memoryClientMock;
        private readonly Mock<IRedisClient> _redisClientMock;
        private readonly AutoMock _autoMock;

        public SetAsyncTests()
        {
            _fixture = new Fixture();
            _autoMock = AutoMock.GetStrict();
            _memoryClientMock = _autoMock.Mock<IMemoryClient>();
            _redisClientMock = _autoMock.Mock<IRedisClient>();
        }

        [AutoData, Theory]
        public async Task Should_Throw_Exception_When_CancellationTokenRequested(string cacheKey)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var multiCacheManager = _autoMock.Create<MultiCacheManager>();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => multiCacheManager.GetAsync<string>(cacheKey, cancellationTokenSource.Token));
        }

        [AutoData, Theory]
        public async Task Should_Set_Cache_Without_TTL(string cacheKey, string value)
        {
            var byteResponse = JsonSerializer.SerializeToUtf8Bytes(value);
            _memoryClientMock.Setup(q => q.Set(cacheKey, value)).Returns(value);
            _redisClientMock.Setup(q => q.SetAsync(cacheKey, It.IsAny<byte[]>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var multiCacheManager = _autoMock.Create<MultiCacheManager>();
            await multiCacheManager.SetAsync(cacheKey, value);

            _memoryClientMock.Verify(q => q.Set(cacheKey, value), Times.Once);
            _redisClientMock.Verify(q => q.SetAsync(cacheKey, byteResponse, null, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [AutoData, Theory]
        public async Task Should_Set_Cache_With_TTL(string cacheKey, string value, TimeSpan timeSpan)
        {
            var byteResponse = JsonSerializer.SerializeToUtf8Bytes(value);
            _memoryClientMock.Setup(q => q.Set(cacheKey, value, timeSpan)).Returns(value);
            _redisClientMock.Setup(q => q.SetAsync(cacheKey, It.IsAny<byte[]>(), timeSpan, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var multiCacheManager = _autoMock.Create<MultiCacheManager>();
            await multiCacheManager.SetAsync(cacheKey, value, timeSpan);

            _memoryClientMock.Verify(q => q.Set(cacheKey, value, timeSpan), Times.Once);
            _redisClientMock.Verify(q => q.SetAsync(cacheKey, byteResponse, timeSpan, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
