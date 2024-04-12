namespace TonWinPkg.Extensions.Caching.Dapr
{
    internal struct ExtendedCacheValue
    {
        internal DateTime? ExpirationTime { get; set; } = default;
        internal int SlidingTtl { get; set; } = default;
        internal string? ValueBase64 { get; set; } = default;

        public ExtendedCacheValue() 
        {

        }

        internal ExtendedCacheValue(DateTime? expirationTime, int slidingTtl, string valueBase64)
        {
            ExpirationTime = expirationTime;
            SlidingTtl = slidingTtl;
            ValueBase64 = valueBase64;
        }
    }
}
