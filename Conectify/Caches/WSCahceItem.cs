using System.Net.WebSockets;

namespace Conectify.Server.Caches;

public class WSCahceItem(WebSocket webSocket)
{
    public WebSocket WebSocket { get; set; } = webSocket;
    public int Count { get; set; } = 1;
}
