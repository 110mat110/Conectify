using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models;
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
///
/// On top of the 24h window it keeps a separate, never-pruned "last known value" per source. A
/// source that has been idle for more than 24h (typical for an actuator nobody has touched in a
/// while) therefore still reports a current value instead of falling off the UI entirely.
/// </summary>
public class DataCachingService : IDataCachingService
{
    private readonly ConcurrentDictionary<Guid, CacheItem<Event>> valueCache = new();

    /// <summary>Newest value ever seen per source, regardless of age. Never pruned.</summary>
    private readonly ConcurrentDictionary<Guid, Event> lastKnownValues = new();

    private readonly ConcurrentDictionary<Guid, byte> loadingSources = new();
    private readonly ConcurrentDictionary<Guid, long> lastLoadAttempt = new();
    private readonly IServiceProvider serviceProvider;
    private readonly IMapper mapper;
    private readonly ILogger<DataCachingService> logger;
    private readonly IDeviceCachingService deviceCachingService;
    private readonly Timer pruneTimer;
    private static readonly TimeSpan window = TimeSpan.FromDays(1);

    /// <summary>How long to wait before re-querying the Event table for a source that yielded nothing.</summary>
    private static readonly TimeSpan loadCooldown = TimeSpan.FromMinutes(5);

    private const long BucketMillis = 2 * 60 * 1000;
    private const int MaxPoints = 720;

    public DataCachingService(IServiceProvider serviceProvider, IMapper mapper, ILogger<DataCachingService> logger, IDeviceCachingService deviceCachingService)
    {
        this.serviceProvider = serviceProvider;
        this.mapper = mapper;
        this.logger = logger;
        this.deviceCachingService = deviceCachingService;

        // Warm the cache in the background so the service is responsive immediately and the first
        // UI request does not wait for the full 24h preload query.
        _ = Task.Run(() =>
        {
            PreloadAllSensors();
            PreloadLastKnownValues();
        });

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

    /// <summary>
    /// Every actuator points at a sensor that carries its value. Those sensors are the ones that
    /// typically go quiet for days, so their newest value is pulled in explicitly - the 24h preload
    /// above never sees them. Deliberately does NOT mark them active in the device cache: an idle
    /// actuator should keep its value, not reappear as a live sensor.
    /// </summary>
    private void PreloadLastKnownValues()
    {
        Tracing.Trace(() =>
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

            var missing = db.Set<Actuator>()
                .AsNoTracking()
                .Select(a => a.SensorId)
                .Distinct()
                .ToList()
                .Where(id => id != Guid.Empty && !lastKnownValues.ContainsKey(id))
                .ToList();

            if (missing.Count == 0)
            {
                return;
            }

            var loaded = LoadNewestValues(db, missing);
            foreach (var value in loaded)
            {
                RememberLastKnown(value);
            }

            logger.LogInformation("Last-known preload: {loaded}/{requested} idle actuator sensor(s) resolved", loaded.Count, missing.Count);
        }, Guid.NewGuid(), "Preloading last known values");
    }

    /// <summary>
    /// Newest value-event per source, of any age. Two set-based queries (newest timestamp per
    /// source, then the rows carrying those timestamps) instead of one Event scan per source.
    /// </summary>
    private static List<Event> LoadNewestValues(ConectifyDb db, IReadOnlyCollection<Guid> sourceIds)
    {
        var ids = sourceIds.ToList();

        var newestPerSource = db.Set<Event>()
            .AsNoTracking()
            .Where(x => x.Type == Constants.Events.Value && ids.Contains(x.SourceId))
            .GroupBy(x => x.SourceId)
            .Select(g => new { SourceId = g.Key, TimeCreated = g.Max(e => e.TimeCreated) })
            .ToList();

        if (newestPerSource.Count == 0)
        {
            return [];
        }

        var wanted = newestPerSource.Select(x => (x.SourceId, x.TimeCreated)).ToHashSet();
        var matchedIds = newestPerSource.Select(x => x.SourceId).ToList();
        var matchedTimes = newestPerSource.Select(x => x.TimeCreated).Distinct().ToList();

        return db.Set<Event>()
            .AsNoTracking()
            .Where(x => x.Type == Constants.Events.Value && matchedIds.Contains(x.SourceId) && matchedTimes.Contains(x.TimeCreated))
            .ToList()
            // The timestamp list is shared across sources, so drop the cross-matches.
            .Where(x => wanted.Contains((x.SourceId, x.TimeCreated)))
            .GroupBy(x => x.SourceId)
            .Select(g => g.Last())
            .ToList();
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

            return mapper.Map<IEnumerable<ApiEvent>>(Downsample(snapshot));
        }, sourceId, "Get data for last 24h"));
    }

    public Task<ApiEvent?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default)
    {
        return Task.FromResult(Tracing.Trace(() =>
        {
            // Falls back to the last known value of any age, so an actuator idle for days still
            // reports its current state instead of nothing.
            var latest = LatestWithinWindow(sourceId) ?? LastKnown(sourceId);
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
                    // Nothing inside the 24h window. If we know an older value, still report it as
                    // the current one (with an empty chart series); otherwise warm the cache up for
                    // the next call and skip this source in this response.
                    var lastKnown = LastKnown(sourceId);
                    if (lastKnown is null)
                    {
                        TriggerBackgroundLoad(sourceId);
                        continue;
                    }

                    result[sourceId] = new SensorHistoryBundle { Latest = mapper.Map<ApiEvent>(lastKnown) };
                    continue;
                }

                result[sourceId] = new SensorHistoryBundle
                {
                    // Downsampled series for the chart, but keep the true latest reading un-averaged
                    // so the cube's current value and accent colour stay exact.
                    Values = mapper.Map<List<ApiEvent>>(Downsample(snapshot)),
                    Latest = mapper.Map<ApiEvent>(snapshot[^1]), // snapshot is ordered ascending by time
                };
            }

            return result;
        }, Guid.NewGuid(), "Get history bundle"));
    }

    private void AppendToCache(Event value)
    {
        RememberLastKnown(value);

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
        foreach (var e in events)
        {
            RememberLastKnown(e);
        }

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

    /// <summary>Keeps <see cref="lastKnownValues"/> at the newest event seen for the source.</summary>
    private void RememberLastKnown(Event value)
    {
        lastKnownValues.AddOrUpdate(
            value.SourceId,
            value,
            (_, current) => value.TimeCreated >= current.TimeCreated ? value : current);
    }

    private Event? LastKnown(Guid sourceId) => lastKnownValues.TryGetValue(sourceId, out var value) ? value : null;

    /// <summary>
    /// Collapses an ascending-by-time list of events into at most one point per 2-minute bucket,
    /// averaging the numeric value of everything that falls in the same bucket. Non-numeric fields
    /// (unit, string value, ...) are taken from the last event in the bucket.
    /// </summary>
    private static List<Event> Downsample(List<Event> ordered)
    {
        if (ordered.Count <= MaxPoints)
        {
            return ordered;
        }

        var result = new List<Event>();
        long currentBucket = ordered[0].TimeCreated / BucketMillis;
        double sum = 0;
        int numericCount = 0;
        Event last = ordered[0];

        foreach (var e in ordered)
        {
            long bucket = e.TimeCreated / BucketMillis;
            if (bucket != currentBucket)
            {
                result.Add(BuildAveraged(last, sum, numericCount));
                currentBucket = bucket;
                sum = 0;
                numericCount = 0;
            }

            if (e.NumericValue.HasValue)
            {
                sum += e.NumericValue.Value;
                numericCount++;
            }
            last = e;
        }

        result.Add(BuildAveraged(last, sum, numericCount));

        return result;
    }

    private static Event BuildAveraged(Event representative, double sum, int numericCount) => new()
    {
        Id = representative.Id,
        SourceId = representative.SourceId,
        DestinationId = representative.DestinationId,
        Type = representative.Type,
        Name = representative.Name,
        Unit = representative.Unit,
        StringValue = representative.StringValue,
        NumericValue = numericCount > 0 ? (float)(sum / numericCount) : null,
        TimeCreated = representative.TimeCreated,
    };

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
        // A source with no recent data is asked for on every UI refresh; without the cooldown that
        // turns into an Event table query per request forever.
        if (lastLoadAttempt.TryGetValue(sourceId, out var previous)
            && Environment.TickCount64 - previous < loadCooldown.TotalMilliseconds)
        {
            return;
        }

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
                lastLoadAttempt[sourceId] = Environment.TickCount64;
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
            return;
        }

        // Silent for over 24h - keep at least its newest value so the source keeps a current value.
        var lastKnown = await db.Set<Event>()
            .AsNoTracking()
            .Where(x => x.Type == Constants.Events.Value && x.SourceId == sourceId)
            .OrderByDescending(x => x.TimeCreated)
            .FirstOrDefaultAsync();

        if (lastKnown is not null)
        {
            RememberLastKnown(lastKnown);
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
