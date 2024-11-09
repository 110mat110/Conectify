using Conectify.Services.Shelly.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Shelly.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WebsocketController(IShellyService shellyService) : ControllerBase
{
    [HttpGet("/ws")]
    public async Task Get(CancellationToken cancellationToken)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await shellyService.ReceiveMessages(webSocket, cancellationToken);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }
}
