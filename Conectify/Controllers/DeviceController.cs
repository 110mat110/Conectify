namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class DeviceController(IDeviceService deviceService, ILogger<DeviceController> logger) : DeviceControllerBase<ApiDevice>(logger, deviceService)
{
}
