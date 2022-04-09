namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService deviceService;
    private readonly ILogger<DeviceController> logger;

    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger)
    {
        this.deviceService = deviceService;
        this.logger = logger;
    }


    [HttpPost()]
    public async Task<IActionResult> AddNewDevice(ApiDevice apiDevice, CancellationToken ct)
    {
        try
        {
            return (await deviceService.AddKnownDevice(apiDevice, ct)) ? Ok() : BadRequest();
            
        }
            catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecificDevice(Guid id, CancellationToken ct= default)
    {
        try
        {
            var result = await deviceService.GetSpecificDevice(id, ct);
            return result != null ? new ObjectResult(result) : NotFound();

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot download device");
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllDevices(CancellationToken ct = default)
    {
        try
        {
            return new ObjectResult(await deviceService.GetAllDevices(ct));

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }
}
