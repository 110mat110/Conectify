namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class DeviceController : DeviceControllerBase<ApiDevice>
{
    public DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger) : base(logger, deviceService)
    {
    }
}
