using Dapr.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TonWinPkg.Extensions.Caching.Dapr.Shared;

namespace TonWinPkg.Extensions.Caching.Dapr
{
    public class DaprCache : IDistributedCache
    {
        private readonly DaprClient _client;

        private readonly DaprCacheOptions _options;

        private readonly ILogger _logger;

        public DaprCache(IOptions<DaprCacheOptions> optionsAccessor)
            : this(optionsAccessor, NullLoggerFactory.Instance.CreateLogger<DaprCache>())
        {

        }

        public DaprCache(IOptions<DaprCacheOptions> optionsAccessor, ILogger logger)
        {
            ArgumentNullThrowHelper.ThrowIfNull(optionsAccessor);
            ArgumentNullThrowHelper.ThrowIfNull(logger);

            _options = optionsAccessor.Value;
            ArgumentNullThrowHelper.ThrowIfNull(_options.StoreName);
            ArgumentNullThrowHelper.ThrowIfNull(_options.HttpEndPoint);

            _logger = logger;

            var builder = new DaprClientBuilder();
            if (_options.HttpEndPoint != null)
            {
                builder.UseHttpEndpoint(_options.HttpEndPoint);
            }
            _client = builder.Build();
        }

        public byte[]? Get(string key)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            token.ThrowIfCancellationRequested();

            var extendedValue = await _client.GetStateAsync<ExtendedCacheValue?>(_options.StoreName, key, null, null, token);

            if (extendedValue.HasValue == false)
            {
                return null;
            }

            if (extendedValue.Value.ValueBase64 == null)
            {
                return null;
            }

            var realValue = Convert.FromBase64String(extendedValue.Value.ValueBase64);

            //If a cache entry uses a sliding expiration time, reset its time-to-live
            if (extendedValue.Value.SlidingTtl != -1)
            {
                int cacheTtl = 0;

                if(extendedValue.Value.ExpirationTime.HasValue 
                    && extendedValue.Value.ExpirationTime > DateTimeOffset.UtcNow)
                {
                    cacheTtl = (int)(extendedValue.Value.ExpirationTime - DateTimeOffset.UtcNow).Value.TotalSeconds;
                }

                cacheTtl = Math.Min(extendedValue.Value.SlidingTtl, cacheTtl);

                if(cacheTtl > 0)
                {
                    var options = new DistributedCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromSeconds(cacheTtl)
                    };
                    await SetAsync(key, Convert.FromBase64String(extendedValue.Value.ValueBase64), options, token);
                }
            }

            return realValue;
        }

        public void Refresh(string key)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            RefreshAsync(key).GetAwaiter().GetResult();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            token.ThrowIfCancellationRequested();

            if (await _client.CheckHealthAsync(token))
            {
                _ = await GetAsync(key, token);
            }
            else
            {
                throw new InvalidOperationException("sidecar is unhealthy.");
            }
        }

        public void Remove(string key)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            RemoveAsync(key).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            token.ThrowIfCancellationRequested();

            if (await _client.CheckHealthAsync(token))
            {
                await _client.DeleteStateAsync(_options.StoreName, key, null, null, token);
            }
            else
            {
                throw new InvalidOperationException("sidecar is unhealthy.");
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);
            ArgumentNullThrowHelper.ThrowIfNull(value);
            ArgumentNullThrowHelper.ThrowIfNull(options);

            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);
            ArgumentNullThrowHelper.ThrowIfNull(value);
            ArgumentNullThrowHelper.ThrowIfNull(options);

            token.ThrowIfCancellationRequested();

            if (await _client.CheckHealthAsync(token))
            {
                //Adding extra information to the value of a cache entry.
                var (cacheTtl,expirationTime, slidingTtl) = CacheTtlCalculateHelper.Calculate(options);
                string valueBase64 = Convert.ToBase64String(value);

                var extendedCacheValue = new ExtendedCacheValue(expirationTime, slidingTtl, valueBase64);

                var metadata = new Dictionary<string, string>();
                //A time-to-live of -1 means that the cache entry will never expire
                metadata.Add("ttlInSeconds", cacheTtl.ToString());

                await _client.SaveStateAsync(_options.StoreName, key, extendedCacheValue, null, metadata, token);
            }
            else
            {
                throw new InvalidOperationException("sidecar is unhealthy.");
            }
        }
    }
}
