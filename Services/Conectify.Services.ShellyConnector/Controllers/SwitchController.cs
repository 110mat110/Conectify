using Conectify.Services.ShellyConnector.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.ShellyConnector.Controllers;

[Route("[controller]")]
[ApiController]
public class SwitchController(IShellyService shellyService) : ControllerBase
{
    [HttpGet("{id}/Off")]
    public async Task<IActionResult> Off(Guid id)
    {
        await shellyService.SetSwitch(id, false);
        return Ok();
    }

    [HttpGet("{id}/On")]
    public async Task<IActionResult> On(Guid id)
    {
        await shellyService.SetSwitch(id,true);

        return Ok();
    }

    [HttpGet("{id}/Trigger")]
    public async Task<IActionResult> Trigger(Guid id)
    {
        await shellyService.Trigger(id);

        return Ok();
    }

    [HttpGet("{id}/LongPress")]
    public async Task<IActionResult> LongPress(Guid id)
    {
        await shellyService.LongPress(id);
        return Ok();
    }
}