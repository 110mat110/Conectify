using Conectify.Service.History.Services;
using Conectify.Shared.Library.Models.Values;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Service.History.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController(ILogger<DataController> logger, IDataCachingService dataCachingService) : ControllerBase
{
    [HttpGet("{sensorId}/values")]
    public async Task<IEnumerable<ApiValue>> Get(Guid sensorId)
    {
        return await dataCachingService.GetDataForLast24h(sensorId);
    }

    [HttpGet("{sensorId}/latest")]
    public async Task<ApiValue?> GetLatestAsync(Guid sensorId)
    {
        return await dataCachingService.GetLatestValueAsync(sensorId);
    }
}