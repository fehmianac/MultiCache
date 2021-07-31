using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MultiCache.Abstractions;

namespace MultiCache.Demo.Net31.Controllers
{
    [Route("api/v1")]
    public class DemoController : ControllerBase
    {
        private readonly IMultiCacheManager _multiCacheManager;
        private readonly IMemoryCache _memoryCache; 
        private readonly string cacheKey = "Demo";

        public DemoController(IMemoryCache memoryCache, IMultiCacheManager multiCacheManager)
        {
            _memoryCache = memoryCache;
            _multiCacheManager = multiCacheManager;
        }

        [HttpGet("memory")]
        public IActionResult GetFromMemory()
        {
            return Ok(_memoryCache.Get<string>(cacheKey));
        }

        [HttpPost("memory")]
        public IActionResult SetToMemory(CancellationToken cancellationToken)
        {
            _memoryCache.Set(cacheKey, "CacheValue", TimeSpan.FromMinutes(10));
            return Ok();
        }
        
        [HttpDelete("memory")]
        public IActionResult RemoveFromMemory()
        {
            _memoryCache.Remove(cacheKey);
            return Ok();
        }

        [HttpPost("multi")]
        public async Task<IActionResult> SetToMulti(CancellationToken cancellationToken)
        {
            await _multiCacheManager.SetAsync(cacheKey, "CacheValue", TimeSpan.FromMinutes(10), cancellationToken);
            return Ok();
        }

        [HttpGet("multi")]
        public async Task<IActionResult> GetFromMulti(CancellationToken cancellationToken)
        {
            return Ok(await _multiCacheManager.GetAsync<string>(cacheKey, cancellationToken));
        }
    }
}
