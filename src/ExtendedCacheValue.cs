namespace Microsoft.Extensions.Caching.Dapr
{
    internal struct ExtendedCacheValue
    {
        public bool IsSlidingExpiration { get; }
        public int TtlInSeconds { get; }
        public string ValueBase64 { get; }

        internal ExtendedCacheValue(bool isSlidingExpiration, int ttlInSeconds, string valueBase64)
        {
            IsSlidingExpiration = isSlidingExpiration;
            TtlInSeconds = ttlInSeconds;
            ValueBase64 = valueBase64;
        }
    }
}
