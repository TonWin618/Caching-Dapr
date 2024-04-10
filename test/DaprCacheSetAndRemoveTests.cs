using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.CompilerServices;
using System.Text;

namespace TonWinPkg.Extensions.Caching.Dapr.Test;

public class DaprCacheSetAndRemoveTests
{
    [Fact]
    public void GetMissingKeyReturnsNull()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        string key = "non-existent-key";

        var result = cache.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void SetAndGetReturnsObject()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var value = new byte[1];
        string key = Me();

        cache.Set(key, value);

        var result = cache.Get(key);
        Assert.Equal(value, result);
    }

    [Fact]
    public void SetAndGetWorksWithCaseSensitiveKeys()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var value = new byte[1];
        string key1 = Me().ToLower();
        string key2 = Me().ToUpper();

        cache.Set(key1, value);

        var result = cache.Get(key1);
        Assert.Equal(value, result);

        result = cache.Get(key2);
        Assert.Null(result);
    }

    [Fact]
    public void SetAlwaysOverwrites()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var value1 = new byte[1] { 1 };
        string key = Me();

        cache.Set(key, value1);
        var result = cache.Get(key);
        Assert.Equal(value1, result);

        var value2 = new byte[1] { 2 };
        cache.Set(key, value2);
        result = cache.Get(key);
        Assert.Equal(value2, result);
    }

    [Fact]
    public void RemoveRemoves()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var value = new byte[1];
        string key = Me();

        cache.Set(key, value);
        var result = cache.Get(key);
        Assert.Equal(value, result);

        cache.Remove(key);
        result = cache.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void SetNullValueThrows()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        byte[] value = null;
        string key = Me();

        Assert.Throws<ArgumentNullException>(() => cache.Set(key, value));
    }

    [Fact]
    public void SetGetEmptyNonNullBuffer()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        cache.Remove(key); // known state
        Assert.Null(cache.Get(key)); // expect null

        cache.Set(key, Array.Empty<byte>());
        var arr = cache.Get(key);
        Assert.NotNull(arr);
        Assert.Empty(arr);
    }

    [Fact]
    public async Task SetGetEmptyNonNullBufferAsync()
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        await cache.RemoveAsync(key); // known state
        Assert.Null(await cache.GetAsync(key)); // expect null

        await cache.SetAsync(key, Array.Empty<byte>());
        var arr = await cache.GetAsync(key);
        Assert.NotNull(arr);
        Assert.Empty(arr);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    public void SetGetNonNullString(string payload)
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        cache.Remove(key); // known state
        Assert.Null(cache.Get(key)); // expect null
        cache.SetString(key, payload);

        // check raw bytes
        var raw = cache.Get(key);
        Assert.Equal(Hex(payload), Hex(raw));

        // check via string API
        var value = cache.GetString(key);
        Assert.NotNull(value);
        Assert.Equal(payload, value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("abc def ghi jkl mno pqr stu vwx yz!")]
    public async Task SetGetNonNullStringAsync(string payload)
    {
        var cache = DaprTestConfig.CreateCacheInstance();
        var key = Me();
        await cache.RemoveAsync(key); // known state
        Assert.Null(await cache.GetAsync(key)); // expect null
        await cache.SetStringAsync(key, payload);

        // check raw bytes
        var raw = await cache.GetAsync(key);
        Assert.Equal(Hex(payload), Hex(raw));

        // check via string API
        var value = await cache.GetStringAsync(key);
        Assert.NotNull(value);
        Assert.Equal(payload, value);
    }

    static string Hex(byte[] value) => BitConverter.ToString(value);
    static string Hex(string value) => Hex(Encoding.UTF8.GetBytes(value));

    private static string Me([CallerMemberName] string caller = "") => caller;
}
