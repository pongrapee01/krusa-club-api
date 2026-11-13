using KrusaClub.Api.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace KrusaClub.Api.Features.Master.Caching;

public sealed class MasterDataCache
{
    private readonly IMemoryCache _cache;
    private readonly MasterCacheOptions _options;

    public MasterDataCache(IMemoryCache cache, IOptions<MasterCacheOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool TryGetList<T>(string cacheKey, out IReadOnlyList<T>? items)
    {
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<T>? cached) && cached is not null)
        {
            items = cached;
            return true;
        }

        items = null;
        return false;
    }

    public void SetList<T>(string cacheKey, IReadOnlyList<T> items)
        => _cache.Set(cacheKey, items, CreateEntryOptions());

    public async Task<IReadOnlyList<T>> GetListAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<IReadOnlyList<T>>> loadFromDb,
        CancellationToken cancellationToken = default)
    {
        if (TryGetList(cacheKey, out IReadOnlyList<T>? cached) && cached is not null)
        {
            return cached;
        }

        var fromDb = await loadFromDb(cancellationToken);
        SetList(cacheKey, fromDb);
        return fromDb;
    }

    public void Invalidate(string cacheKey) => _cache.Remove(cacheKey);

    private MemoryCacheEntryOptions CreateEntryOptions()
    {
        var entry = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                Math.Max(1, _options.AbsoluteExpirationMinutes))
        };

        if (_options.SlidingExpirationMinutes > 0)
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(_options.SlidingExpirationMinutes);
        }

        return entry;
    }
}
