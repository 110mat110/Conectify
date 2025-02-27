using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Service.History.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Values;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
    private readonly IDictionary<Guid, CacheItem<Event>> valueCache = new Dictionary<Guid, CacheItem<Event>>(); //TODO check if I should not use Concurency dictionary here
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
                    valueCache.Add(valueGroup.Key, new CacheItem<Event>());
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

            await ReloadCache(value.SourceId, value.Id, ct);

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

    private async Task ReloadCache(Guid sensorId, Guid traceId, CancellationToken ct = default)
    {
        await Tracing.Trace(async () =>
        {
            if (valueCache.ContainsKey(sensorId) && DateTime.UtcNow.Subtract(valueCache[sensorId].CreationTimeUtc).TotalMilliseconds > cacheDurationMillis)
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
            logger.LogInformation("Preloading cache from time {time}", yesterdayUnixTime);
            using var scope = this.serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
            var values = await db.Set<Event>().Where(x => x.Type == Constants.Events.Value && x.SourceId == sensorId && x.TimeCreated > yesterdayUnixTime).ToListAsync(ct);

            if (!values.Any())
            {
                return;
            }

            lock (locker)
            {
                valueCache.Add(sensorId, new CacheItem<Event>());
                valueCache[sensorId].AddRange(values);
                valueCache[sensorId].Reorder(x => x.TimeCreated);
            }
        }, traceId, "Cache reload");
    }

    public async Task<IEnumerable<ApiEvent>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default)
    {
        return await Tracing.Trace(async () =>
        {
            await ReloadCache(sourceId, sourceId, ct);
            return mapper.Map<IEnumerable<ApiEvent>>(valueCache[sourceId]);
        }, sourceId, "Get data for last 24h");
    }

    public async Task<ApiEvent?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default)
    {
        await ReloadCache(sourceId, sourceId, ct);

        if (valueCache.ContainsKey(sourceId))
        {
            var value = valueCache[sourceId].OrderByDescending(x => x.TimeCreated).FirstOrDefault();
            return mapper.Map<ApiEvent>(value);
        }
        return null;
    }
}
