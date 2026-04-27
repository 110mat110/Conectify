using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Service.History.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Values;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Service.History.Services;

public interface IDataCachingService
{
    Task InsertValue(Event websocketValue, CancellationToken ct = default);
    Task<IEnumerable<ApiEvent>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default);

    Task<ApiEvent?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default);
}

public class DataCachingService : IDataCachingService
{
    private readonly object locker = new();
    private readonly Dictionary<Guid, CacheItem<Event>> valueCache = []; //TODO check if I should not use Concurency dictionary here
    private readonly IServiceProvider serviceProvider;
    private readonly IMapper mapper;
    private readonly ILogger<DataCachingService> logger;
    private readonly IDeviceCachingService deviceCachingService;
    private static readonly double cacheDurationMillis = 1000 * 60 * 15;

    public DataCachingService(IServiceProvider serviceProvider, IMapper mapper, ILogger<DataCachingService> logger, IDeviceCachingService deviceCachingService)
    {
        this.serviceProvider = serviceProvider;
        this.mapper = mapper;
        this.logger = logger;
        this.deviceCachingService = deviceCachingService;
        PreloadAllSensors();
    }

    private void PreloadAllSensors()
    {
        Tracing.Trace(() =>
        {
            lock (locker)
            {
                var yesterdayUnixTime = DateTimeOffset.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0)).ToUnixTimeMilliseconds();
                logger.LogInformation("Preloading all active sensors to the cache from time {time}", yesterdayUnixTime);
                using var scope = this.serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
                var groups = db.Set<Event>().Where(x => x.TimeCreated > yesterdayUnixTime && x.Type == Constants.Events.Value).ToList().GroupBy(x => x.SourceId);


                foreach (var valueGroup in groups)
                {
                    deviceCachingService.ObserveSensorFromEvent(valueGroup.Last());
                    valueCache.Add(valueGroup.Key, []);
                    valueCache[valueGroup.Key].AddRange(valueGroup);
                    valueCache[valueGroup.Key].Reorder(x => x.TimeCreated);
                }
            }
        }, Guid.NewGuid(), "Preloading");
    }

    public async Task InsertValue(Event value, CancellationToken ct = default)
    {
        await Tracing.Trace(async () =>
        {

            if (value.Type != Constants.Events.Value)
            {
                return;
            }

            // keep populating cache using the existing boundary behavior (load full window)
            await ReloadCache(value.SourceId, value.Id, loadLatest: false, ct);

            if (valueCache.ContainsKey(value.SourceId))
            {
                if (!(valueCache[value.SourceId].Any(x => x.NumericValue == value.NumericValue && x.TimeCreated == value.TimeCreated)))
                {
                    lock (locker)
                    {
                        valueCache[value.SourceId].Add(value);
                    }
                }
            }
            else
            {
                lock (locker)
                {
                    valueCache.Add(value.SourceId, new CacheItem<Event>(value));
                }
            }
        }, value.Id, "Insert value");
    }

    // loadLatest = true -> only load single latest item from DB (cheap)
    // loadLatest = false -> load full 24h window into cache (current behaviour)
    private async Task ReloadCache(Guid sensorId, Guid traceId, bool loadLatest = false, CancellationToken ct = default)
    {
        await Tracing.Trace(async () =>
        {
            if (valueCache.TryGetValue(sensorId, out CacheItem<Event>? value) && DateTime.UtcNow.Subtract(value.CreationTimeUtc).TotalMilliseconds > cacheDurationMillis)
            {
                lock (locker)
                {
                    valueCache.Remove(sensorId);
                }
            }

            if (valueCache.ContainsKey(sensorId))
            {
                return;
            }

            var yesterdayUnixTime = DateTimeOffset.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0)).ToUnixTimeMilliseconds();
            logger.LogInformation("Preloading cache from time {time} (loadLatest={loadLatest})", yesterdayUnixTime, loadLatest);
            using var scope = this.serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

            if (loadLatest)
            {
                // only fetch the most recent row for this sensor (fast)
                var latest = await db.Set<Event>()
                    .AsNoTracking()
                    .Where(x => x.Type == Constants.Events.Value && x.SourceId == sensorId && x.TimeCreated > yesterdayUnixTime)
                    .OrderByDescending(x => x.TimeCreated)
                    .Select(x => new Event
                    {
                        Id = x.Id,
                        SourceId = x.SourceId,
                        Type = x.Type,
                        Name = x.Name,
                        NumericValue = x.NumericValue,
                        StringValue = x.StringValue,
                        Unit = x.Unit,
                        TimeCreated = x.TimeCreated,
                    })
                    .FirstOrDefaultAsync(ct);

                if (latest is null)
                {
                    return;
                }

                lock (locker)
                {
                    if (!valueCache.ContainsKey(sensorId))
                    {
                        valueCache.Add(sensorId, new CacheItem<Event>(latest));
                    }
                    else
                    {
                        valueCache[sensorId].Add(latest);
                        valueCache[sensorId].Reorder(x => x.TimeCreated);
                    }
                }

                // notify device cache about observation
                deviceCachingService.ObserveSensorFromEvent(latest);
                return;
            }

            // existing (full) preload: load entire 24h window
            var values = await db.Set<Event>()
                .AsNoTracking()
                .Where(x => x.Type == Constants.Events.Value && x.SourceId == sensorId && x.TimeCreated > yesterdayUnixTime)
                .OrderBy(x => x.TimeCreated)
                .ToListAsync(ct);

            if (values.Count == 0)
            {
                return;
            }

            lock (locker)
            {
                if (valueCache.TryAdd(sensorId, []))
                {
                    valueCache[sensorId].AddRange(values);
                    valueCache[sensorId].Reorder(x => x.TimeCreated);
                }
            }

            // notify device cache about the last observed event
            deviceCachingService.ObserveSensorFromEvent(values.Last());
        }, traceId, "Cache reload");
    }

    public async Task<IEnumerable<ApiEvent>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default)
    {
        return await Tracing.Trace(async () =>
        {
            // attempt to use cache first (fast)
            if (valueCache.TryGetValue(sourceId, out CacheItem<Event>? cached))
            {
                return mapper.Map<IEnumerable<ApiEvent>>(cached);
            }

            // cache miss -> project DB rows directly to ApiEvent (no full entity materialization)
            var yesterdayUnixTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1)).ToUnixTimeMilliseconds();
            using var scope = this.serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

            var projected = await db.Set<Event>()
                .AsNoTracking()
                .Where(x => x.Type == Constants.Events.Value && x.SourceId == sourceId && x.TimeCreated > yesterdayUnixTime)
                .OrderBy(x => x.TimeCreated)
                .ProjectTo<ApiEvent>(mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return projected;
        }, sourceId, "Get data for last 24h");
    }

    public async Task<ApiEvent?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default)
    {
        return await Tracing.Trace(async () =>
        {
            // prefer in-memory cache for fastest access
            if (valueCache.TryGetValue(sourceId, out CacheItem<Event>? cacheitem) && cacheitem.Any())
            {
                var value = cacheitem.OrderByDescending(x => x.TimeCreated).FirstOrDefault();
                return mapper.Map<ApiEvent>(value);
            }

            // cache miss -> load only latest row via ReloadCache(loadLatest: true)
            await ReloadCache(sourceId, sourceId, loadLatest: true, ct);

            if (valueCache.TryGetValue(sourceId, out CacheItem<Event>? cacheAfter))
            {
                var value = cacheAfter.OrderByDescending(x => x.TimeCreated).FirstOrDefault();
                return mapper.Map<ApiEvent>(value);
            }

            return null;
        }, sourceId, "Get latest value");
    }
}
