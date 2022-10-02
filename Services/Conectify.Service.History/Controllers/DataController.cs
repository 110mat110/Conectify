using Conectify.Service.History.Services;
using Conectify.Shared.Library.Models.Values;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Service.History.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> _logger;
    private readonly IDataCachingService dataCachingService;

    public DataController(ILogger<DataController> logger, IDataCachingService dataCachingService)
    {
        _logger = logger;
        this.dataCachingService = dataCachingService;
    }

    [HttpGet("{sensorId}/values")]
    public async Task<IEnumerable<ApiValue>> Get(Guid sensorId)
    {
        return await dataCachingService.GetDataForLast24h(sensorId);
    }

    [HttpGet("{sensorId}/latest")]
    public ApiValue? GetLatest(Guid sensorId)
    {
        return dataCachingService.GetLatestValue(sensorId);
    }
}