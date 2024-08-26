using Conectify.Services.ShellyConnector.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.ShellyConnector.Controllers;

[Route("[controller]")]
[ApiController]
public class SwitchController(IShellyService shellyService) : ControllerBase
{
    [HttpGet("Off")]
    public async Task<IActionResult> Off()
    {
        await shellyService.SetSwitch(false);
        return Ok();
    }

    [HttpGet("On")]
    public async Task<IActionResult> On()
    {
        await shellyService.SetSwitch(true);

        return Ok();
    }

    [HttpGet("LongPress")]
    public async Task<IActionResult> LongPress()
    {
        await shellyService.LongPress();
        return Ok();
    }
}