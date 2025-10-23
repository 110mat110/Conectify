using Conectify.Services.SmartThings.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.SmartThings.Controllers;
[Route("api/[controller]")]
[ApiController]
public class Testing(SmartThingsService smartThingsService)
{
    [HttpGet("Temp")]
    public async Task Get()
    {
        await smartThingsService.RefreshAllCapabilities(default);
    }

    [HttpGet("Init")]
    public async Task Init()
    {
        await smartThingsService.RegisterAllDevices(default);
    }
}
