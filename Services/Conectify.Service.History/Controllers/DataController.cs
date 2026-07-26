using Conectify.Service.History.Services;
using Conectify.Shared.Library.Models.Values;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Service.History.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController(IDataCachingService dataCachingService) : ControllerBase
{
    [HttpGet("{sensorId}/values")]
    public async Task<IEnumerable<ApiEvent>> Get(Guid sensorId)
    {
        return await dataCachingService.GetDataForLast24h(sensorId);
    }

    [HttpGet("{sensorId}/latest")]
    public async Task<ApiEvent?> GetLatestAsync(Guid sensorId)
    {
        return await dataCachingService.GetLatestValueAsync(sensorId);
    }

    [HttpPost("latest-batch")]
    public async Task<Dictionary<Guid, ApiEvent>> GetLatestBatch([FromBody] List<Guid> sensorIds, CancellationToken ct)
    {
        var tasks = sensorIds.Select(async id =>
        {
            var latest = await dataCachingService.GetLatestValueAsync(id, ct);
            return (id, latest);
        });

        var results = await Task.WhenAll(tasks);
        return results
            .Where(r => r.latest is not null)
            .ToDictionary(r => r.id, r => r.latest!);
    }

    /// <summary>
    /// One call returns the latest value AND the last 24h of values for every requested source,
    /// served entirely from the in-memory cache (no Event table access).
    /// </summary>
    [HttpPost("bundle")]
    public async Task<Dictionary<Guid, SensorHistoryBundle>> GetBundle([FromBody] List<Guid> sourceIds, CancellationToken ct)
    {
        return await dataCachingService.GetBundleAsync(sourceIds, ct);
    }
}