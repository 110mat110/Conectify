using Conectify.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UpdateController(IUpdateService updateService) : ControllerBase
{
    [HttpGet("version/{deviceId}/{chipType}/{version}")]
    public async Task RegisterVersionAsync(Guid deviceId, string version, string chipType, CancellationToken cancellationToken)
    {
        await updateService.RegisterDeviceAsync(deviceId, version, chipType,  cancellationToken);
    }

    [HttpGet("url/{deviceId}")]
    public async Task<string> GetUrlAsync(Guid deviceId,  CancellationToken cancellationToken)
    {
       return await updateService.GetLatestVersionUrl(deviceId, cancellationToken);
    }

    [HttpGet("updated/{deviceId}")]
    public async Task Updated(Guid deviceId)
    {
        await updateService.ChipUpdated(deviceId);
    }

    [HttpGet("manualScrape")]
    public async Task<string> Scrape()
    {
        return await updateService.GetLatestVersionFromGit();
    }
}
