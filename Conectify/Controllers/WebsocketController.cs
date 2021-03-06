namespace Conectify.Server.Controllers;

using Conectify.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class WebsocketController : ControllerBase
{
    private readonly IWebSocketService webSocketService;

    public WebsocketController(IWebSocketService webSocketService)
    {
        this.webSocketService = webSocketService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> WebsocketRegister(Guid id)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

            await webSocketService.ConnectAsync(id, ws);

            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpPost("{id}/direct")]
    public async Task<IActionResult> DirectWriteToSocket(Guid id, string message)
    {
        await webSocketService.SendToThingAsync(id, message);
        return Ok();
    }
}
