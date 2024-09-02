namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SensorsController(ISensorService deviceService, ILogger<SensorsController> logger) : DeviceControllerBase<ApiSensor>(logger, deviceService)
{
    [HttpGet("by-device/{id}")]
    public async Task<IActionResult> GetAllDevices(Guid id, CancellationToken ct = default)
    {
        try
        {
            return new ObjectResult(await deviceService.GetAllSensorsPerDevice(id, ct));

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }

    [HttpGet("by-actuator/{id}")]
    public async Task<IActionResult> SensorByActuator(Guid id, CancellationToken ct = default)
    {
        try
        {
            return new ObjectResult(await deviceService.GetSensorByActuator(id, ct));

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }

    [HttpGet("lastValue/{id}")]
    public async Task<IActionResult> LastValue(Guid id, CancellationToken ct = default)
    {
        return new ObjectResult(await deviceService.GetLastValue(id, ct));
    }
}
