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
}