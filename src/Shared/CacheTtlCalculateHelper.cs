using Microsoft.Extensions.Caching.Distributed;

namespace TonWinPkg.Extensions.Caching.Dapr.Shared
{
    internal static class CacheTtlCalculateHelper
    {
        /// <summary>
        /// Calculated as time to live based on the cache entry option
        /// </summary>
        /// <param name="options">Distributed Cache Entry Options</param>
        /// <returns>(isSlidingExpiration, time-to-live)</returns>
        /// <exception cref="ArgumentOutOfRangeException">The absolute expiration value must be in the future.</exception>
        internal static (bool, int) CalculateTtlSeconds(DistributedCacheEntryOptions options)
        {
            int ttl;
            if (options.AbsoluteExpiration.HasValue)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration.Value.UtcDateTime <= now)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                        options.AbsoluteExpiration.Value,
                        "The absolute expiration value must be in the future.");
                }
                ttl = (int)(options.AbsoluteExpiration.Value.UtcDateTime - now).TotalSeconds;
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
