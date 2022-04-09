namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ActuatorsController : ControllerBase
{
    private readonly IActuatorService ActuatorService;
    private readonly ILogger<DeviceController> logger;

    public ActuatorsController(IActuatorService deviceService, ILogger<DeviceController> logger)
    {
        this.ActuatorService = deviceService;
        this.logger = logger;
    }


    [HttpPost()]
    public async Task<IActionResult> AddNewDevice(ApiActuator apiDevice, CancellationToken ct)
    {
        try
        {
            return (await ActuatorService.AddKnownActuator(apiDevice, ct)) ? Ok() : BadRequest();

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
            var result = await ActuatorService.GetActuator(id, ct);
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
            return new ObjectResult(await ActuatorService.GetAllActuatorsPerDevice(id, ct));

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }
}
