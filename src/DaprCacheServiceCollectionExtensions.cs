using Microsoft.Extensions.Caching.Dapr;
using Microsoft.Extensions.Caching.Dapr.Shared;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.DependencyInjection;

public static class StackExchangeRedisCacheServiceCollectionExtensions
{
    /// <summary>
    /// Extends the <see cref="IServiceCollection"/> to add a distributed cache service using Dapr.
    /// This method sets up the necessary services and configurations for using Dapr's cache
    /// functionality within the application. It allows for the customization of DaprCacheOptions
    /// through the <paramref name="setupAction"/> delegate.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> instance to add the distributed cache service to.
    /// </param>
    /// <param name="setupAction">
    /// A delegate of type <see cref="Action{DaprCacheOptions}"/> that is used to configure
    /// and customize the <see cref="DaprCacheOptions"/>.
    /// </param>
    /// <returns>
    /// An instance of <see cref="IServiceCollection"/> with the distributed cache service added and configured.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="services"/> or <paramref name="setupAction"/> is null.
    /// </exception>
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