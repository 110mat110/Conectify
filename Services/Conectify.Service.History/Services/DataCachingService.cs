using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Service.History.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Values;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Conectify.Service.History.Services;

public interface IDataCachingService
{
    Task InsertValue(Event websocketValue, CancellationToken ct = default);
    Task<IEnumerable<ApiEvent>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default);

    Task<ApiEvent?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default);

    Task<Dictionary<Guid, SensorHistoryBundle>> GetBundleAsync(IEnumerable<Guid> sourceIds, CancellationToken ct = default);
}

/// <summary>
/// Keeps the last 24h of value-events for every active source in memory so UI requests can be
/// answered instantly. The request path NEVER touches the (slow) Event table - it is warmed once
/// at startup in the background, kept fresh by incoming websocket values, and back-filled
/// asynchronously on a cache miss.
/// </summary>
public class DataCachingService : IDataCachingService
{
    private readonly ConcurrentDictionary<Guid, CacheItem<Event>> valueCache = new();
    private readonly ConcurrentDictionary<Guid, byte> loadingSources = new();
    private readonly IServiceProvider serviceProvider;
    private readonly IMapper mapper;
    private readonly ILogger<DataCachingService> logger;
    private readonly IDeviceCachingService deviceCachingService;
    private readonly Timer pruneTimer;
    private static readonly TimeSpan window = TimeSpan.FromDays(1);

    public DataCachingService(IServiceProvider serviceProvider, IMapper mapper, ILogger<DataCachingService> logger, IDeviceCachingService deviceCachingService)
    {
        this.serviceProvider = serviceProvider;
        this.mapper = mapper;
        this.logger = logger;
        this.deviceCachingService = deviceCachingService;

        // Warm the cache in the background so the service is responsive immediately and the first
        // UI request does not wait for the full 24h preload query.
        _ = Task.Run(PreloadAllSensors);

        // Drop values that have aged out of the 24h window so the cache stays bounded.
        pruneTimer = new Timer(_ => PruneOldValues(), null, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
    }

    private static long WindowStartUnix() => DateTimeOffset.UtcNow.Subtract(window).ToUnixTimeMilliseconds();

    private void PreloadAllSensors()
    {
        Tracing.Trace(() =>
        {
            var windowStart = WindowStartUnix();
            logger.LogInformation("Preloading all active sensors into the cache from time {time}", windowStart);
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

            var groups = db.Set<Event>()
                .AsNoTracking()
                .Where(x => x.TimeCreated > windowStart && x.Type == Constants.Events.Value)
                .ToList()
                .GroupBy(x => x.SourceId);

            foreach (var valueGroup in groups)
            {
                deviceCachingService.ObserveSensorFromEvent(valueGroup.Last());
                MergeIntoCache(valueGroup.Key, valueGroup.ToList());
            }

            logger.LogInformation("Preload complete: {count} source(s) cached", valueCache.Count);
        }, Guid.NewGuid(), "Preloading");
    }

    public Task InsertValue(Event value, CancellationToken ct = default)
    {
        Tracing.Trace(() =>
        {
            if (value.Type != Constants.Events.Value)
            {
                return;
            }

            AppendToCache(value);
        }, value.Id, "Insert value");

        return Task.CompletedTask;
    }

    public Task<IEnumerable<ApiEvent>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default)
    {
        return Task.FromResult(Tracing.Trace(() =>
        {
            var snapshot = SnapshotWithinWindow(sourceId);
            if (snapshot.Count == 0)
            {
                // Nothing cached yet - back-fill in the background and return what we have (nothing)
                // so the request stays fast. Websocket values keep filling it in the meantime.
                TriggerBackgroundLoad(sourceId);
                return Enumerable.Empty<ApiEvent>();
            }

            return mapper.Map<IEnumerable<ApiEvent>>(snapshot);
        }, sourceId, "Get data for last 24h"));
    }

    public Task<ApiEvent?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default)
    {
        return Task.FromResult(Tracing.Trace(() =>
        {
            var latest = LatestWithinWindow(sourceId);
            if (latest is null)
            {
                TriggerBackgroundLoad(sourceId);
                return (ApiEvent?)null;
            }

            return mapper.Map<ApiEvent>(latest);
        }, sourceId, "Get latest value"));
    }

    public Task<Dictionary<Guid, SensorHistoryBundle>> GetBundleAsync(IEnumerable<Guid> sourceIds, CancellationToken ct = default)
    {
        return Task.FromResult(Tracing.Trace(() =>
        {
            var result = new Dictionary<Guid, SensorHistoryBundle>();

            foreach (var sourceId in sourceIds.Distinct())
            {
                var snapshot = SnapshotWithinWindow(sourceId);
                if (snapshot.Count == 0)
                {
                    // Not cached yet - warm it up for the next call, skip it in this response.
                    TriggerBackgroundLoad(sourceId);
                    continue;
                }

                var values = mapper.Map<List<ApiEvent>>(snapshot);
                result[sourceId] = new SensorHistoryBundle
                {
                    Values = values,
                    Latest = values[^1], // snapshot is ordered ascending by time
                };
            }

            return result;
        }, Guid.NewGuid(), "Get history bundle"));
    }

    private void AppendToCache(Event value)
    {
        var item = valueCache.GetOrAdd(value.SourceId, _ =>
        {
            // First time we see this source since startup - pull its recent history in the background.
            TriggerBackgroundLoad(value.SourceId);
            return new CacheItem<Event>();
        });

        lock (item)
        {
            if (!item.Any(x => x.NumericValue == value.NumericValue && x.TimeCreated == value.TimeCreated))
            {
                item.Add(value);
            }
        }
    }

    private void MergeIntoCache(Guid sourceId, IReadOnlyCollection<Event> events)
    {
        var item = valueCache.GetOrAdd(sourceId, _ => new CacheItem<Event>());

        lock (item)
        {
            if (item.Count == 0)
            {
                item.AddRange(events);
                return;
            }

            var existing = new HashSet<(long, float?)>(item.Select(x => (x.TimeCreated, x.NumericValue)));
            foreach (var e in events)
            {
                if (existing.Add((e.TimeCreated, e.NumericValue)))
                {
                    item.Add(e);
                }
            }
        }
    }

    private List<Event> SnapshotWithinWindow(Guid sourceId)
    {
        if (!valueCache.TryGetValue(sourceId, out var item))
        {
            return [];
        }

        var windowStart = WindowStartUnix();
        lock (item)
        {
            return item.Where(x => x.TimeCreated > windowStart).OrderBy(x => x.TimeCreated).ToList();
        }
    }

    private Event? LatestWithinWindow(Guid sourceId)
    {
        if (!valueCache.TryGetValue(sourceId, out var item))
        {
            return null;
        }

        var windowStart = WindowStartUnix();
        lock (item)
        {
            return item.Where(x => x.TimeCreated > windowStart).OrderByDescending(x => x.TimeCreated).FirstOrDefault();
        }
    }

    private void TriggerBackgroundLoad(Guid sourceId)
    {
        if (!loadingSources.TryAdd(sourceId, 0))
        {
            return; // already loading
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await BackgroundLoad(sourceId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background history load failed for {sourceId}", sourceId);
            }
            finally
            {
                loadingSources.TryRemove(sourceId, out _);
            }
        });
    }

    private async Task BackgroundLoad(Guid sourceId)
    {
        var windowStart = WindowStartUnix();
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

        var values = await db.Set<Event>()
            .AsNoTracking()
            .Where(x => x.Type == Constants.Events.Value && x.SourceId == sourceId && x.TimeCreated > windowStart)
            .OrderBy(x => x.TimeCreated)
            .ToListAsync();

        if (values.Count > 0)
        {
            MergeIntoCache(sourceId, values);
        }
    }

    private void PruneOldValues()
    {
        var windowStart = WindowStartUnix();
        foreach (var item in valueCache.Values)
        {
            lock (item)
            {
                item.RemoveAll(x => x.TimeCreated <= windowStart);
            }
        }
    }
}
