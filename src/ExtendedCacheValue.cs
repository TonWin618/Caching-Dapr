using System.Diagnostics;

namespace TonWinPkg.Extensions.Caching.Dapr
{
    internal struct ExtendedCacheValue
    {
        public bool IsSlidingExpiration { get; set; }
        public int TtlInSeconds { get; set; }
        public string ValueBase64 { get; set; }

        internal ExtendedCacheValue(bool isSlidingExpiration, int ttlInSeconds, string valueBase64)
        {
            IsSlidingExpiration = isSlidingExpiration;
            TtlInSeconds = ttlInSeconds;
            ValueBase64 = valueBase64;
        }
    }
}
