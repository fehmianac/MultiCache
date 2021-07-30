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
    public class GetAsyncTests
    {
        private readonly Fixture _fixture;

        private readonly Mock<IMemoryClient> _memoryClientMock;
        private readonly Mock<IRedisClient> _redisClientMock;
        private readonly AutoMock _autoMock;

        public GetAsyncTests()
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
            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => multiCacheManager.GetAsync<string>(cacheKey, cancellationTokenSource.Token));
        }

        [AutoData, Theory]
        public async Task Should_Return_From_Memory_If_Cache_Exist_In_Memory(string cacheKey)
        {
            var response = _fixture.Create<string>();
            _memoryClientMock.Setup(q => q.TryGetValue(cacheKey, out response)).Returns(true);

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            var cachedResult = await multiCacheManager.GetAsync<string>(cacheKey);

            Assert.Equal(response, cachedResult);
            _memoryClientMock.Verify(q => q.TryGetValue(cacheKey, out response), Times.Once);
        }

        [AutoData, Theory]
        public async Task Should_Return_Empty_If_Cache_Doesnt_Exist_In_Memory_And_Redis(string cacheKey)
        {
            var response = _fixture.Create<string>();
            _memoryClientMock.Setup(q => q.TryGetValue(cacheKey, out response)).Returns(false);
            _redisClientMock.Setup(q => q.GetWithExpiryAsync(cacheKey, default)).ReturnsAsync(() => (null, TimeSpan.Zero));

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            var cachedResult = await multiCacheManager.GetAsync<string>(cacheKey);

            Assert.Equal(default, cachedResult);

            _memoryClientMock.Verify(q => q.TryGetValue(cacheKey, out response), Times.Once);
            _redisClientMock.Verify(q => q.GetWithExpiryAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [AutoData, Theory]
        public async Task Should_Return_From_Redis_And_Set_Memory_With_TTL_When_Cache_Exist_In_Redis(string cacheKey)
        {
            var response = _fixture.Create<string>();
            var redisTtl = _fixture.Create<TimeSpan>();

            _memoryClientMock.Setup(q => q.TryGetValue(cacheKey, out response)).Returns(false);
            _memoryClientMock.Setup(q => q.Set(cacheKey, response, redisTtl)).Returns(response);
            _redisClientMock.Setup(q => q.GetWithExpiryAsync(cacheKey, default)).ReturnsAsync(() => (JsonSerializer.SerializeToUtf8Bytes(response), redisTtl));

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            var cachedResult = await multiCacheManager.GetAsync<string>(cacheKey);

            Assert.Equal(response, cachedResult);
            _memoryClientMock.Verify(q => q.TryGetValue(cacheKey, out response), Times.Once);
            _memoryClientMock.Verify(q => q.Set(cacheKey, response, redisTtl), Times.Once);
            _redisClientMock.Verify(q => q.GetWithExpiryAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [AutoData, Theory]
        public async Task Should_Return_From_Redis_And_Set_Memory_Without_TTL_When_Cache_Exist_In_Redis(string cacheKey)
        {
            var response = _fixture.Create<string>();

            _memoryClientMock.Setup(q => q.TryGetValue(cacheKey, out response)).Returns(false);
            _memoryClientMock.Setup(q => q.Set(cacheKey, response)).Returns(response);
            _redisClientMock.Setup(q => q.GetWithExpiryAsync(cacheKey, default)).ReturnsAsync(() => (JsonSerializer.SerializeToUtf8Bytes(response), null));

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            var cachedResult = await multiCacheManager.GetAsync<string>(cacheKey);

            Assert.Equal(response, cachedResult);
            _memoryClientMock.Verify(q => q.TryGetValue(cacheKey, out response), Times.Once);
            _memoryClientMock.Verify(q => q.Set(cacheKey, response), Times.Once);
            _redisClientMock.Verify(q => q.GetWithExpiryAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
