using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.Dapr.Shared
{
    internal static class CacheTtlCalculateHelper
    {
        internal static (bool, int) CalculateTtlSeconds(DistributedCacheEntryOptions options)
        {
            int ttl;
            if (options.AbsoluteExpiration.HasValue)
            {
                DateTimeOffset now = DateTimeOffset.Now;
                ttl = (int)(options.AbsoluteExpiration.Value - now).TotalSeconds;
                return (false, ttl);
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                ttl = (int)options.AbsoluteExpirationRelativeToNow.Value.TotalSeconds;
                return (false, ttl);
            }
            else if (options.SlidingExpiration.HasValue)
            {
                ttl = (int)options.SlidingExpiration.Value.TotalSeconds;
                return (true, ttl);
            }
            else
            {
                // If none of the expiration parameters are provided, return -1 indicating no expiration.
                return (false, -1);
            }
        }

        internal static DistributedCacheEntryOptions ToSlidingExpirationOption(int ttl)
        {
            return new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(ttl)
            };
        }
    }
}
