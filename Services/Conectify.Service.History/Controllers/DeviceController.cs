using Conectify.Service.History.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Service.History.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceCachingService deviceCachingService;

        public DeviceController(IDeviceCachingService deviceCachingService)
        {
            this.deviceCachingService = deviceCachingService;
        }

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
    }
}
