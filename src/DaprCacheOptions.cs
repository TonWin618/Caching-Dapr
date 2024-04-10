using Microsoft.Extensions.Options;

namespace TonWinPkg.Extensions.Caching.Dapr
{
    public class DaprCacheOptions : IOptions<DaprCacheOptions>
    {
        /// <summary>
        /// Name of the State Store Component.
        /// After Dapr is initialized, the name used for the state store component is "statestore".
        /// </summary>
        public string? StoreName { get; set; }

        /// <summary>
        /// Dapr's Http endpoint, If empty the default endpoint will be used: http://localhost:3500
        /// </summary>
        public string? HttpEndPoint { get; set; }

        DaprCacheOptions IOptions<DaprCacheOptions>.Value
        {
            get { return this; }
        }
    }
}
