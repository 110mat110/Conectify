namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class SensorsController : ControllerBase
{
    private readonly ISensorService sensorService;
    private readonly ILogger<DeviceController> logger;

    public SensorsController(ISensorService deviceService, ILogger<DeviceController> logger)
    {
        this.sensorService = deviceService;
        this.logger = logger;
    }


    [HttpPost()]
    public async Task<IActionResult> AddNewDevice(ApiSensor apiDevice, CancellationToken ct)
    {
        try
        {
            return (await sensorService.AddKnownSensor(apiDevice, ct)) ? Ok() : BadRequest();

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecificDevice(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await sensorService.GetSensor(id, ct);
            return result != null ? new ObjectResult(result) : NotFound();

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot download device");
        }
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
