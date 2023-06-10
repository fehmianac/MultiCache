using Microsoft.AspNetCore.Mvc;
using MultiCache.Abstractions;

namespace MultiCache.Demo.Net60.Handler;

public class Demo
{
    public static async Task<IResult> Handler([FromServices] IMultiCacheManager multiCacheManager,CancellationToken cancellationToken)
    {
        var cacheKey = "demoCacheKey";
        var (cachedValue,fromMemory) = await multiCacheManager.GetAsync<string>(cacheKey, cancellationToken);
        if (cachedValue != null)
        {
            return Results.Ok("Cached Value: " + cachedValue + " From Memory: " + fromMemory);
        }
        
        var newValue = Guid.NewGuid().ToString();
        await multiCacheManager.SetAsync(cacheKey, newValue, TimeSpan.FromMinutes(1), cancellationToken);
        return Results.Ok("New Value: " + newValue);
        
    }
}