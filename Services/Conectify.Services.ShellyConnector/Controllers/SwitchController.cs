using Conectify.Services.ShellyConnector.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.ShellyConnector.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SwitchController : ControllerBase
    {
        private readonly IShellyService shellyService;

        public SwitchController(IShellyService shellyService)
        {
            this.shellyService = shellyService;
        }

        [HttpGet("Off")]
        public string Off()
        {
            shellyService.SetSwitch(false);
            return "Hello";
        }

        [HttpGet("On")]
        public string On()
        {
            shellyService.SetSwitch(true);

            return "Hello";
        }
    }
}