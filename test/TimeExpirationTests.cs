using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.CompilerServices;

namespace TonWinPkg.Extensions.Caching.Dapr.Test;

public class TimeExpirationTests
{
    [Fact]
    public void AbsoluteExpirationInThePastThrows()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        var expected = DateTimeOffset.Now - TimeSpan.FromMinutes(1);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            cache.Set(key, value, new DistributedCacheEntryOptions().SetAbsoluteExpiration(expected));
        });

        Assert.StartsWith("The absolute expiration value must be in the future.", ex.Message);
    }

    [Fact]
    public void AbsoluteExpirationExpires()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(1)));

        byte[] result = cache.Get(key);
        Assert.Equal(value, result);

        for (int i = 0; i < 4 && result != null; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            result = cache.Get(key);
        }

        Assert.Null(result);
    }

    [Fact]
    public void AbsoluteSubSecondExpirationExpiresImmidately()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(0.25)));

        var result = cache.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void NegativeRelativeExpirationThrows()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            cache.Set(key, value, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(-1)));
        });

        Assert.StartsWith("The relative expiration value must be positive.", ex.Message);
    }

    [Fact]
    public void ZeroRelativeExpirationThrows()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            cache.Set(key, value, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.Zero));
        });

        Assert.StartsWith("The relative expiration value must be positive.", ex.Message);
    }

    [Fact]
    public void RelativeExpirationExpires()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(1)));

        var result = cache.Get(key);
        Assert.Equal(value, result);

        for (int i = 0; i < 4 && result != null; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
            result = cache.Get(key);
        }
        Assert.Null(result);
    }

    [Fact]
    public void RelativeSubSecondExpirationExpiresImmediately()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(0.25)));

        var result = cache.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void NegativeSlidingExpirationThrows()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            cache.Set(key, value, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(-1)));
        });

        Assert.StartsWith("The sliding expiration value must be positive.", ex.Message);
    }

    [Fact]
    public void ZeroSlidingExpirationThrows()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            cache.Set(key, value, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.Zero));
        });

        Assert.StartsWith("The sliding expiration value must be positive.", ex.Message);
    }

    [Fact]
    public void SlidingExpirationExpiresIfNotAccessed()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(1)));

        var result = cache.Get(key);
        Assert.Equal(value, result);

        Thread.Sleep(TimeSpan.FromSeconds(3));

        result = cache.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void SlidingSubSecondExpirationExpiresImmediately()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(0.25)));

        var result = cache.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void SlidingExpirationRenewedByAccess()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(1)));

        var result = cache.Get(key);
        Assert.Equal(value, result);

        for (int i = 0; i < 5; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            result = cache.Get(key);
            Assert.Equal(value, result);
        }

        Thread.Sleep(TimeSpan.FromSeconds(3));
        result = cache.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void SlidingExpirationRenewedByAccessUntilAbsoluteExpiration()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        var value = new byte[1];

        cache.Set(key, value, new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(1))
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(3)));

        var result = cache.Get(key);
        Assert.Equal(value, result);

        for (int i = 0; i < 5; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            result = cache.Get(key);
            Assert.Equal(value, result);
        }

        Thread.Sleep(TimeSpan.FromSeconds(.6));

        result = cache.Get(key);
        Assert.Null(result);
    }

    private static string Me([CallerMemberName] string caller = "") => caller;
}
