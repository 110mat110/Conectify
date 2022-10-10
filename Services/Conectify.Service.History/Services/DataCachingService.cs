using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Service.History.Models;
using Conectify.Shared.Library.Models.Values;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Service.History.Services;

public interface IDataCachingService
{
    Task InsertValue(Value websocketValue, CancellationToken ct = default);
    Task<IEnumerable<ApiValue>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default);

    Task<ApiValue?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default);
}

public class DataCachingService : IDataCachingService
{
    private readonly object locker = new object();
    private readonly IDictionary<Guid, CacheItem<Value>> valueCache = new Dictionary<Guid, CacheItem<Value>>(); //TODO check if I should not use Concurency dictionary here
    private readonly IServiceProvider serviceProvider;
    private readonly IMapper mapper;
    private readonly ILogger<DataCachingService> logger;
    private readonly IDeviceCachingService deviceCachingService;
    private static double cacheDurationMillis = 1000*60*15;

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
        lock (locker) { 
            var yesterdayUnixTime = DateTimeOffset.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0)).ToUnixTimeMilliseconds();
            logger.LogInformation("Preloading all active sensors to the cache from time " + yesterdayUnixTime);
            using var scope = this.serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
            var groups = db.Set<Value>().Where(x => x.TimeCreated > yesterdayUnixTime).ToList().GroupBy(x => x.SourceId);


            foreach (var valueGroup in groups)
            {
                deviceCachingService.ObserveSensorFromValue(valueGroup.Last());
                valueCache.Add(valueGroup.Key, new CacheItem<Value>());
                valueCache[valueGroup.Key].AddRange(valueGroup);
                valueCache[valueGroup.Key].Reorder(x => x.TimeCreated);
            }
        }
    }

    public async Task InsertValue(Value value, CancellationToken ct = default)
    {
        TryInvalidateCache(value.SourceId);

        if (valueCache.ContainsKey(value.SourceId))
        {
            lock (locker)
            {
                valueCache[value.SourceId].Add(value);
            }
        }
        else
        {
            lock (locker)
            {
                valueCache.Add(value.SourceId, new CacheItem<Value>(value));
            }
            await PreloadValueCache(value.SourceId, ct);
        }
    }

    private void TryInvalidateCache(Guid id)
    {
        if(valueCache.ContainsKey(id) && DateTime.UtcNow.Subtract(valueCache[id].CreationTimeUtc).TotalMilliseconds > cacheDurationMillis)
        {
            lock (locker)
            {
                valueCache.Remove(id);
            }
        }
    }

    private async Task PreloadValueCache(Guid sensorId, CancellationToken ct = default)
    {
        TryInvalidateCache(sensorId);
        if (valueCache.ContainsKey(sensorId))
        {
            return;
        }

        var yesterdayUnixTime = DateTimeOffset.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0)).ToUnixTimeMilliseconds();
        logger.LogInformation("Preloading cache from time " + yesterdayUnixTime);
        using var scope = this.serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();
        var values = await db.Set<Value>().Where(x => x.SourceId == sensorId && x.TimeCreated > yesterdayUnixTime).ToListAsync(ct);

        if (!values.Any())
        {
            return;
        }

        lock (locker)
        {
            if (!valueCache.ContainsKey(sensorId))
            {
                valueCache.Add(sensorId, new CacheItem<Value>());
            }
            valueCache[sensorId].AddRange(values);
            valueCache[sensorId].Reorder(x => x.TimeCreated);
        }
    }

    public async Task<IEnumerable<ApiValue>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default)
    {
        TryInvalidateCache(sourceId);
        if (!valueCache.ContainsKey(sourceId))
        {
            await PreloadValueCache(sourceId, ct);
        }
        return mapper.Map<IEnumerable<ApiValue>>(valueCache[sourceId]);
    }

    public async Task<ApiValue?> GetLatestValueAsync(Guid sourceId, CancellationToken ct = default)
    {
        await PreloadValueCache(sourceId, ct);

        if (valueCache.ContainsKey(sourceId))
        {
            var value = valueCache[sourceId].OrderByDescending(x => x.TimeCreated).FirstOrDefault();
            return mapper.Map<ApiValue>(value);
        }
        return null;
    }
}
