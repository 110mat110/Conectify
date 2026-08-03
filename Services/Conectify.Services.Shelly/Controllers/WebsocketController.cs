using Conectify.Services.Shelly.Services;
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
            // The device dials in, so its LAN address is right here - it is what the dashboard
            // links to until the device reports its own wifi.sta_ip.
            var remote = HttpContext.Connection.RemoteIpAddress;
            if (remote is not null && remote.IsIPv4MappedToIPv6)
            {
                remote = remote.MapToIPv4();
            }

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await shellyService.ReceiveMessages(webSocket, remote?.ToString(), cancellationToken);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }
}
