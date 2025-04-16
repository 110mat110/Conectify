using Conectify.Service.History.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Service.History.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DeviceController(IDeviceCachingService deviceCachingService) : ControllerBase
{
    [HttpGet("sensors")]
    public IEnumerable<Guid> ActiveSensors()
    {
        return deviceCachingService.GetActiveSensors();
    }

    [HttpGet("actuators")]
    public IEnumerable<Guid> ActiveActuators()
    {
        return deviceCachingService.GetActiveActuators();
    }

    [HttpGet("reset")]
    public IActionResult Reset()
    {
        deviceCachingService.Reset();
        return Ok();
    }
}
