namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ActuatorsController : DeviceControllerBase<ApiActuator>
{
    private readonly IActuatorService service;
    private readonly ILogger<ActuatorsController> logger;

    public ActuatorsController(IActuatorService service, ILogger<ActuatorsController> logger) : base(logger, service)
    {
        this.service = service;
        this.logger = logger;
    }

    /// <summary>
    /// Mintly test
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet("by-device/{id}")]
    public async Task<IActionResult> GetAllDevices(Guid id, CancellationToken ct = default)
    {
        try
        {
            return new ObjectResult(await service.GetAllActuatorsPerDevice(id, ct));

        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return this.Problem("Cannot upload device");
        }
    }
}
