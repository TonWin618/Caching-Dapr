using Dapr.Client;
using Google.Protobuf;
using Microsoft.Extensions.Caching.Dapr.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.Dapr
{
    public class DaprCache : IDistributedCache
    {
        private readonly DaprClient _client;

        private readonly DaprCacheOptions _options;

        private readonly ILogger _logger;

        public DaprCache(IOptions<DaprCacheOptions> optionsAccessor)
            : this(optionsAccessor, Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<DaprCache>())
        {

        }

        public DaprCache(IOptions<DaprCacheOptions> optionsAccessor, ILogger logger) 
        {
            ArgumentNullThrowHelper.ThrowIfNull(optionsAccessor);
            ArgumentNullThrowHelper.ThrowIfNull(logger);

            _options = optionsAccessor.Value;
            ArgumentNullThrowHelper.ThrowIfNull(_options.StoreName);
            ArgumentNullThrowHelper.ThrowIfNull(_options.DaprEndPoint);

            _logger = logger;

            var builder = new DaprClientBuilder().UseHttpEndpoint(_options.DaprEndPoint);
            _client = builder.Build();
        }

        public byte[]? Get(string key)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            var asyncTask = GetAsync(key);
            asyncTask.ConfigureAwait(false);
            return asyncTask.Result;
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            token.ThrowIfCancellationRequested();

            var extendedValue = await _client.GetStateAsync<ByteString>(_options.StoreName, key, null, null, token);
            var extendedValueString  = extendedValue.ToString();

            if(extendedValueString == null)
            {
                return null;
            }

            var (isSlidingExpiration, ttlInSeconds, realValue) = CacheSerializationHelper.DeserializeCacheValue(extendedValueString);

            if (isSlidingExpiration)
            {
                var options = CacheTtlCalculateHelper.ToSlidingExpirationOption(ttlInSeconds);
                await SetAsync(key, [], options);
            }

            return realValue;
        }

        public void Refresh(string key)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            var asyncTask = RefreshAsync(key);
            asyncTask.ConfigureAwait(false);
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

            var asyncTask = RemoveAsync(key);
            asyncTask.ConfigureAwait(false);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);

            token.ThrowIfCancellationRequested();

            if (await _client.CheckHealthAsync(token))
            {
                await _client.DeleteStateAsync(_options.StoreName, key,null,null,token);
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
            
            var asyncTask = SetAsync(key,value,options);

            asyncTask.ConfigureAwait(false);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            ArgumentNullThrowHelper.ThrowIfNull(key);
            ArgumentNullThrowHelper.ThrowIfNull(value);
            ArgumentNullThrowHelper.ThrowIfNull(options);

            token.ThrowIfCancellationRequested();
            
            if (await _client.CheckHealthAsync(token))
            {
                var (isSlidingExpiration, ttl) = CacheTtlCalculateHelper.CalculateTtlSeconds(options);
                var valueString = CacheSerializationHelper.SerilizeCacheValue(isSlidingExpiration, ttl, value);

                var metadata = new Dictionary<string, string>();
                metadata.Add("ttlInSeconds", ttl.ToString());

                await _client.SaveStateAsync(_options.StoreName, key, valueString, null, metadata, token);
            }
            else
            {
                throw new InvalidOperationException("sidecar is unhealthy.");
            }
        }
    }
}
