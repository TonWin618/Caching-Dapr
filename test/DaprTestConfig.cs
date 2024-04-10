using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.Dapr.Test;

internal class DaprTestConfig
{
    public static IDistributedCache CreateCacheInstance()
    {
        return new DaprCache(new DaprCacheOptions()
        {
            StoreName = "statestore",
            HttpEndPoint = "http://localhost:3500"
        });
    }
}
