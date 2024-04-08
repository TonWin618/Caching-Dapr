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

            var result = await _client.GetStateAsync<ByteString>(_options.StoreName, key, null, null, token);
            var array = result.ToArray();
            var (isSlidingExpiration, ttl) = GetCacheHeader(ref array);

            if (isSlidingExpiration)
            {
                var options = CacheTtlCalculateHelper.ToSlidingExpirationOption(ttl);
                await SetAsync(key, [], options);
            }

            return result.Skip(1).ToArray();
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
                await _client.GetMetadataAsync();
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
                SetCacheHeader(ref value, isSlidingExpiration, ttl);

                var metadata = new Dictionary<string, string>();
                metadata.Add("ttlInSeconds", ttl.ToString());

                await _client.SaveStateAsync(_options.StoreName, key, value, null, metadata, token);
            }
            else
            {
                throw new InvalidOperationException("sidecar is unhealthy.");
            }
        }

        private static void SetCacheHeader(ref byte[] array, bool isSlidingExpiration, int ttl)
        {
            byte[] newArray = new byte[array.Length + 5];

            byte[] boolBytes = BitConverter.GetBytes(isSlidingExpiration);
            byte[] intBytes = BitConverter.GetBytes(ttl);
            byte[] combinedBytes = new byte[5];

            Buffer.BlockCopy(boolBytes, 0, combinedBytes, 0, 1);
            Buffer.BlockCopy(intBytes, 0, combinedBytes, 1, 4);

            Array.Copy(combinedBytes, 0, newArray, 0, 5);
            Array.Copy(array, 0, newArray, 5, array.Length);

            array = newArray;
        }

        private static (bool,int) GetCacheHeader(ref byte[] array)
        {
            byte[] result = new byte[5];
            Array.Copy(array, 0, result, 0, 5);

            bool isSlidingExpiration = BitConverter.ToBoolean(result, 0);
            int ttl = BitConverter.ToInt32(result, 1);

            byte[] newArray = new byte[array.Length - 5];
            Array.Copy(array, 5, newArray, 0, array.Length - 5);
            array = newArray;

            return (isSlidingExpiration,ttl);
        }
    }
}
