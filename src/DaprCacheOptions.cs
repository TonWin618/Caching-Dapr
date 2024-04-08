using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.Dapr
{
    public class DaprCacheOptions: IOptions<DaprCacheOptions>
    {
        public string? StoreName { get; set; }

        public string? DaprEndPoint { get; set; }

        DaprCacheOptions IOptions<DaprCacheOptions>.Value
        {
            get { return this; }
        }
    }
}
