namespace TonWinPkg.Extensions.Caching.Dapr
{
    internal struct ExtendedCacheValue
    {
        public DateTime? ExpirationTime { get; set; } = default;
        public int SlidingTtl { get; set; } = default;
        public string ValueBase64 { get; set; } = "";

        public ExtendedCacheValue()
        {

        }

        public ExtendedCacheValue(DateTime? expirationTime, int slidingTtl, string valueBase64)
        {
            ExpirationTime = expirationTime;
            SlidingTtl = slidingTtl;
            ValueBase64 = valueBase64;
        }
    }
}
