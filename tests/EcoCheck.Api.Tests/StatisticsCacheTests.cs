using EcoCheck.Api.Services;
using Microsoft.Extensions.Caching.Memory;

namespace EcoCheck.Api.Tests;

public class StatisticsCacheTests
{
    [Fact]
    public void Filter_CacheKey_IsDistinctPerRegion()
    {
        var keys = new[]
        {
            StatisticsFilter.None.CacheKey,
            new StatisticsFilter("BR").CacheKey,
            new StatisticsFilter("BR", "SP").CacheKey,
            new StatisticsFilter("PT").CacheKey
        };

        Assert.Equal(keys.Length, keys.Distinct().Count());
        Assert.False(StatisticsFilter.None.IsActive);
        Assert.True(new StatisticsFilter("BR").IsActive);
    }

    [Fact]
    public void Signal_Reset_ExpiresEntriesOfAllFilters()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var signal = new StatisticsCacheSignal();

        foreach (var key in new[] { "statistics:*:*", "statistics:BR:SP" })
        {
            using var entry = cache.CreateEntry(key);
            entry.Value = key;
            entry.AddExpirationToken(signal.CreateToken());
        }

        Assert.True(cache.TryGetValue("statistics:BR:SP", out _));

        signal.Reset();

        Assert.False(cache.TryGetValue("statistics:*:*", out _));
        Assert.False(cache.TryGetValue("statistics:BR:SP", out _));
    }

    [Fact]
    public void Signal_EntriesCreatedAfterReset_RemainCached()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var signal = new StatisticsCacheSignal();
        signal.Reset();

        using (var entry = cache.CreateEntry("statistics:*:*"))
        {
            entry.Value = "novo";
            entry.AddExpirationToken(signal.CreateToken());
        }

        Assert.True(cache.TryGetValue("statistics:*:*", out _));
    }
}
