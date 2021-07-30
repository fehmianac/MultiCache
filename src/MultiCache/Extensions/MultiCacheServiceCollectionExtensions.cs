using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MultiCache.Abstractions;
using MultiCache.Memory;
using MultiCache.Redis;

namespace MultiCache.Extensions
{
    public static class MultiCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Memory caching, Redis caching and Multi cache services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action"/> to configure the provided
        /// <see cref="RedisClientOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddCacheServices(this IServiceCollection services, Action<RedisClientOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.AddMemoryCache();
            services.AddSingleton<IMemoryClient, MemoryClient>();
            services.AddSingleton<IRedisClient, RedisClient>();
            services.AddSingleton<IMultiCacheManager, MultiCacheManager>();
            services.AddHostedService<CacheInvalidationService>();
            
            return services;
        }
    }
}