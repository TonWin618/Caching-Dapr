using Microsoft.Extensions.Caching.Dapr;
using Microsoft.Extensions.Caching.Dapr.Shared;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.DependencyInjection;

public static class StackExchangeRedisCacheServiceCollectionExtensions
{
    public static IServiceCollection AddDaprCache(this IServiceCollection services, Action<DaprCacheOptions> setupAction)
    {
        ArgumentNullThrowHelper.ThrowIfNull(services);
        ArgumentNullThrowHelper.ThrowIfNull(setupAction);

        services.AddOptions();

        services.Configure(setupAction);
        services.Add(ServiceDescriptor.Singleton<IDistributedCache, DaprCache>());

        return services;
    }
}