using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MultiCache.Redis
{
    public class RedisClientOptions : IOptions<RedisClientOptions>
    {
        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public string Configuration { get; set; }
        
        /// <summary>
        /// The configuration used to connect to Redis.
        /// This is preferred over Configuration.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// The Redis instance name.
        /// </summary>
        public string InstanceName { get; set; }

        RedisClientOptions IOptions<RedisClientOptions>.Value => this;
    }
}
