using System.Text.Json;

namespace Microsoft.Extensions.Caching.Dapr.Shared
{
    internal static class CacheSerializationHelper
    {
        internal static string SerilizeCacheValue(bool isSlidingExpiration, int ttl, byte[] value)
        {
            string valueBase64 = Convert.ToBase64String(value);

            var data = new ExtendedCacheValue(isSlidingExpiration, ttl, valueBase64);

            return JsonSerializer.Serialize(data);
        }

        internal static (bool, int, byte[]) DeserializeCacheValue(string value)
        {
            var extendedValue = JsonSerializer.Deserialize<ExtendedCacheValue>(value);
            var realValue = Convert.FromBase64String(extendedValue.ValueBase64);
            return (extendedValue.IsSlidingExpiration, extendedValue.TtlInSeconds, realValue);
        }

        private struct ExtendedCacheValue
        {
            public bool IsSlidingExpiration { get; }
            public int TtlInSeconds { get; }
            public string ValueBase64 { get; }

            public ExtendedCacheValue(bool isSlidingExpiration, int ttlInSeconds, string valueBase64)
            {
                IsSlidingExpiration = isSlidingExpiration;
                TtlInSeconds = ttlInSeconds;
                ValueBase64 = valueBase64;
            }
        }
    }
}
