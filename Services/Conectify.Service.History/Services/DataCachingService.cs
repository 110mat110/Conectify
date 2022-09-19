using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Service.History.Models;
using Conectify.Shared.Library.Models.Values;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Conectify.Service.History.Services
{
    public interface IDataCachingService
    {
        Task InsertValue(Value websocketValue, CancellationToken ct = default);
        Task<IEnumerable<ApiValue>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default);

        ApiValue? GetLatestValue(Guid sourceId);
    }

    public class DataCachingService : IDataCachingService
    {
        private readonly IDictionary<Guid, CacheItem<Value>> valueCache = new Dictionary<Guid, CacheItem<Value>>(); //TODO check if I should not use Concurency dictionary here
        private readonly IServiceProvider serviceProvider;
        private readonly IMapper mapper;
        private static double cacheDurationMillis = 1000*60*15;

        public DataCachingService(IServiceProvider serviceProvider, IMapper mapper)
        {
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
        }

        public async Task InsertValue(Value value, CancellationToken ct = default)
        {
            TryInvalidateCache(value.SourceId);

            if (valueCache.ContainsKey(value.SourceId))
            {
                valueCache[value.SourceId].Add(value);
            }
            else
            {
                valueCache.Add(value.SourceId, new CacheItem<Value>(value));
                await PreloadValueCache(value.SourceId);
            }
        }

        private void TryInvalidateCache(Guid id)
        {
            if(valueCache.ContainsKey(id) && DateTime.UtcNow.Subtract(valueCache[id].CreationTimeUtc).TotalMilliseconds > cacheDurationMillis)
            {
                valueCache.Remove(id);
            }
        }

        private async Task PreloadValueCache(Guid sensorId, CancellationToken ct = default)
        {
            var yesterdayUnixTime = DateTimeOffset.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0)).ToUnixTimeMilliseconds();
            using var scope = this.serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

            var values = await db.Set<Value>().Where(x => x.SourceId == sensorId && x.TimeCreated > yesterdayUnixTime).ToListAsync(ct);

            if (!valueCache.ContainsKey(sensorId))
            {
                valueCache.Add(sensorId, new CacheItem<Value>());
            }
            valueCache[sensorId].AddRange(values);
            valueCache[sensorId].Reorder(x => x.TimeCreated);
        }

        public async Task<IEnumerable<ApiValue>> GetDataForLast24h(Guid sourceId, CancellationToken ct = default)
        {
            TryInvalidateCache(sourceId);
            if (!valueCache.ContainsKey(sourceId))
            {
                valueCache.Add(sourceId, new CacheItem<Value>());
                await PreloadValueCache(sourceId, ct);
            }
            return mapper.Map<IEnumerable<ApiValue>>(valueCache[sourceId]);
        }

        public ApiValue? GetLatestValue(Guid sourceId)
        {
            if (valueCache.ContainsKey(sourceId))
            {
                var value = valueCache[sourceId].OrderByDescending(x => x.TimeCreated).FirstOrDefault();
                return mapper.Map<ApiValue>(value);
            }
            return null;
        }
    }
}
