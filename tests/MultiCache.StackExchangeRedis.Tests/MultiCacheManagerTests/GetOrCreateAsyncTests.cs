using System;
using System.Text.Json;
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
    public class GetOrCreateAsyncTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IMemoryClient> _memoryClientMock;
        private readonly Mock<IRedisClient> _redisClientMock;
        private readonly AutoMock _autoMock;

        public GetOrCreateAsyncTests()
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
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => multiCacheManager.GetOrCreateAsync(cacheKey, async () => await Task.FromResult(Guid.NewGuid().ToString()), TimeSpan.Zero, cancellationTokenSource.Token));
        }

        [AutoData, Theory]
        public async Task Should_Return_Data_When_Cache_Exist_In_Cache(string cacheKey)
        {
            var response = _fixture.Create<string>();
            _memoryClientMock.Setup(q => q.TryGetValue(cacheKey, out response)).Returns(true);

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            var cachedResult = await multiCacheManager.GetOrCreateAsync(cacheKey, async () => await Task.FromResult(Guid.NewGuid().ToString()), TimeSpan.Zero);

            Assert.Equal(response, cachedResult);
            _memoryClientMock.Verify(q => q.TryGetValue(cacheKey, out response), Times.Once);
        }

        [AutoData, Theory]
        public async Task Should_Return_Null_When_Cacheable_Data_Is_Null(string cacheKey)
        {
            var response = _fixture.Create<string>();
            _memoryClientMock.Setup(q => q.TryGetValue(cacheKey, out response)).Returns(false);
            _redisClientMock.Setup(q => q.GetWithExpiryAsync(cacheKey, It.IsAny<CancellationToken>())).ReturnsAsync(() => (null, null));

            static Task<string> Func()
            {
                return Task.FromResult((string) null);
            }

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            var cachedResult = await multiCacheManager.GetOrCreateAsync(cacheKey, Func, TimeSpan.Zero);

            Assert.Null(cachedResult);
            _memoryClientMock.Verify(q => q.TryGetValue(cacheKey, out response), Times.Once);
            _redisClientMock.Verify(q => q.GetWithExpiryAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [AutoData, Theory]
        public async Task Should_Return_And_Set_Data_When_Data_Is_Not_Null(string cacheKey)
        {
            var response = _fixture.Create<string>();
            var byteResponse = JsonSerializer.SerializeToUtf8Bytes(response);
            var timeSpan = _fixture.Create<TimeSpan>();

            _memoryClientMock.Setup(q => q.TryGetValue(cacheKey, out response)).Returns(false);
            _memoryClientMock.Setup(q => q.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).Returns(response);

            _redisClientMock.Setup(q => q.GetWithExpiryAsync(cacheKey, It.IsAny<CancellationToken>())).ReturnsAsync(() => (null, null));
            _redisClientMock.Setup(q => q.SetAsync(cacheKey, byteResponse, timeSpan, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Task<string> Func()
            {
                return Task.FromResult(response);
            }

            var multiCacheManager = _autoMock.Create<RedisMultiCacheManager>();
            var cachedResult = await multiCacheManager.GetOrCreateAsync(cacheKey, Func, timeSpan);

            Assert.Equal(response, cachedResult);
            _memoryClientMock.Verify(q => q.TryGetValue(cacheKey, out response), Times.Once);
            _memoryClientMock.Verify(q => q.Set(cacheKey, response, timeSpan), Times.Once);
            _redisClientMock.Verify(q => q.GetWithExpiryAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
            _redisClientMock.Verify(q => q.SetAsync(cacheKey, byteResponse, timeSpan, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}