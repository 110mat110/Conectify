namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SubscribeController(IPipelineService pipelineService) : ControllerBase
{
    [HttpGet("{deviceId}/all")]
    public async Task<IActionResult> SubscribeToAll(Guid deviceId)
    {
        await pipelineService.SetSubscribeToAll(deviceId, true);
        return Ok();
    }

    [HttpDelete("{deviceId}/all")]
    public async Task<IActionResult> UnsubscribeToAll(Guid deviceId)
    {
        await pipelineService.SetSubscribeToAll(deviceId, false);
        return Ok();
    }

    [HttpPost("{deviceId}")]
    public async Task<IActionResult> AddSubscribePreference(Guid deviceId, ApiPreferences apiPreferences)
    {
        await pipelineService.SetPreference(deviceId, apiPreferences.Preferences);

        return Ok();
    }
}
