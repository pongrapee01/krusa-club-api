using KrusaClub.Api.Features.Master.Repositories;

namespace KrusaClub.Api.Features.Master.Caching;

public sealed class MasterCacheWarmupHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MasterDataCache _cache;
    private readonly ILogger<MasterCacheWarmupHostedService> _logger;

    public MasterCacheWarmupHostedService(
        IServiceScopeFactory scopeFactory,
        MasterDataCache cache,
        ILogger<MasterCacheWarmupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Warming master data cache on startup...");

        await using var scope = _scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await WarmupAsync(
            MasterCacheKeys.Titles,
            services.GetRequiredService<ITitleRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.Roles,
            services.GetRequiredService<IMasterRoleRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.Semesters,
            services.GetRequiredService<ISemesterRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.Classes,
            services.GetRequiredService<IClassRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.Periods,
            services.GetRequiredService<IPeriodRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.Subjects,
            services.GetRequiredService<ISubjectRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.AttendanceStatuses,
            services.GetRequiredService<IAttendanceStatusRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.SystemStatuses,
            services.GetRequiredService<ISystemStatusRepository>().GetAllAsync,
            cancellationToken);
        await WarmupAsync(
            MasterCacheKeys.Menus,
            services.GetRequiredService<IMasterMenuRepository>().GetAllAsync,
            cancellationToken);

        _logger.LogInformation("Master data cache warmup finished.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task WarmupAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<IReadOnlyList<T>>> loadFromDb,
        CancellationToken cancellationToken)
    {
        try
        {
            var items = await loadFromDb(cancellationToken);
            _cache.SetList(cacheKey, items);
            _logger.LogInformation(
                "Master cache loaded: {CacheKey} ({ItemCount} items)",
                cacheKey,
                items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Master cache warmup skipped for {CacheKey}; requests will load from database on demand",
                cacheKey);
        }
    }
}
