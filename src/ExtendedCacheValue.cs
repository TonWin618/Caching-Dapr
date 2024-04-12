namespace TonWinPkg.Extensions.Caching.Dapr
{
    internal struct ExtendedCacheValue
    {
        public DateTimeOffset? ExpirationTime { get; set; }
        public int SlidingTtl { get; set; }
        public string ValueBase64 { get; set; }

        internal ExtendedCacheValue(DateTimeOffset? expirationTime, int slidingTtl, string valueBase64)
        {
            ExpirationTime = expirationTime;
            SlidingTtl = slidingTtl;
            ValueBase64 = valueBase64;
        }
    }
}
