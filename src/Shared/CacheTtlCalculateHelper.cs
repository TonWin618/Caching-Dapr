using Microsoft.Extensions.Caching.Distributed;

namespace TonWinPkg.Extensions.Caching.Dapr.Shared
{
    internal static class CacheTtlCalculateHelper
    {
        /// <summary>
        /// Calculated as time to live based on the cache entry option
        /// </summary>
        /// <param name="options">Distributed Cache Entry Options</param>
        /// <returns>
        /// <para>cacheTtl: when returning -1, it indicates that the cache entry will not expire.</para>
        /// <para>expirationTime: not null only when both sliding expiration time and absolute expiration time are set.</para>
        /// <para>slidingTtl: when returning -1, it indicates that no sliding expiration time is set.</para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">The absolute expiration value must be in the future.</exception>
        internal static (int cacheTtl, DateTime? expirationTime, int slidingTtl) Calculate(DistributedCacheEntryOptions options)
        {
            int slidingTtl = -1;
            int absoluteTtl = -1;

            //SlidingExpiration
            if (options.SlidingExpiration.HasValue)
            {
                slidingTtl = (int)options.SlidingExpiration.Value.TotalSeconds;
            }

            int absoluteTtlFirst = -1;
            //AbsoluteExpiration
            if (options.AbsoluteExpiration.HasValue)
            {
                if (options.AbsoluteExpiration.Value.UtcDateTime <= DateTime.UtcNow)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                        options.AbsoluteExpiration.Value,
                        "The absolute expiration value must be in the future.");
                }
                absoluteTtlFirst = (int)(options.AbsoluteExpiration.Value.UtcDateTime - DateTime.UtcNow).TotalSeconds;
            }

            int absoluteTtlSecond = -1;
            //AbsoluteExpirationRelativeToNow
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteTtlSecond = (int)options.AbsoluteExpirationRelativeToNow.Value.TotalSeconds;
            }

            absoluteTtl = Math.Max(absoluteTtlFirst, absoluteTtlSecond);

            //When the sliding expiration exceeds the absolute expiration, it becomes meaningless.
            if (absoluteTtl == -1 && slidingTtl == -1)
            {
                return (-1, null, -1);
            }
            //only absoluteTtl
            else if (absoluteTtl != -1 && slidingTtl == -1)
            {
                return (absoluteTtl, null, slidingTtl);
            }
            //only slidingTtl
            else if (absoluteTtl == -1 && slidingTtl != -1)
            {
                return(slidingTtl, null, slidingTtl);
            }
            //both of absoluteTtl and slidingTtl
            else
            {
                if(absoluteTtl > slidingTtl)
                {
                    return(slidingTtl, DateTime.UtcNow.AddSeconds(absoluteTtl), slidingTtl);
                }
                else
                {
                    //When the sliding expiration exceeds the absolute expiration, it becomes meaningless.
                    return (absoluteTtl, null, -1);
                }
            }
        }
    }
}
