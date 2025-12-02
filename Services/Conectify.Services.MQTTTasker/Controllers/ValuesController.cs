using Conectify.Services.MQTTTasker.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.MQTTTasker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValuesController(IValueService valueService) : ControllerBase
{
    [HttpPost("action/{id}/{value}")]
    public async Task<bool> Set(Guid id, float value)
    {
        return await valueService.SetAction(id, value);
    }
}
