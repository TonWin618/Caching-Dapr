using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.Caching.Dapr.Test;

internal class DaprTestConfig
{
    public static IDistributedCache CreateCacheInstance()
    {
        return new DaprCache(new DaprCacheOptions()
        {
            StoreName = "statestore",
            DaprEndPoint = "http://localhost:3500"
        });
    }
}
