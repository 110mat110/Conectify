using System.Net.WebSockets;

namespace Conectify.Server.Caches
{
    public class WSCahceItem
    {
        public WSCahceItem(WebSocket webSocket)
        {
            WebSocket = webSocket;
            Count = 1;
        }
        public WebSocket WebSocket { get; set; }
        public int Count { get; set; }
    }
}
