using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ValuesController(IDataService dataService) : ControllerBase
{
    [HttpPost("{deviceId}")]
    public async Task<IActionResult> SaveValue([FromQuery]Guid deviceId, ApiDataModel dataModel, CancellationToken cancellationToken)
    {
        var res = await dataService.ProcessData(dataModel, deviceId, cancellationToken);

        return Ok(res);
    }
}
