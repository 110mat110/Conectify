namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SensorsController : DeviceControllerBase<ApiSensor>
{
    private readonly ISensorService sensorService;
    private readonly ILogger<SensorsController> logger;

    public SensorsController(ISensorService deviceService, ILogger<SensorsController> logger) : base(logger, deviceService)
    {
        this.sensorService = deviceService;
        this.logger = logger;
    }

    [HttpGet("by-device/{id}")]
    public async Task<IActionResult> GetAllDevices(Guid id, CancellationToken ct = default)
    {
        try
        {
            return new ObjectResult(await sensorService.GetAllSensorsPerDevice(id, ct));

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
            return new ObjectResult(await sensorService.GetSensorByActuator(id, ct));

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }
}
