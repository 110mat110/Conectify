namespace Conectify.Server.Caches;

using System.Net.WebSockets;

public interface IWebsocketCache
{
    void AddNewWebsocket(Guid deviceId, WebSocket webSocket);
    WebSocket? GetActiveSocket(Guid deviceId);
    void Remove(Guid deviceId);
}

public class WebsocketCache : IWebsocketCache
{
    private Dictionary<Guid, WebSocket> sockets = new Dictionary<Guid, WebSocket>();

    public WebsocketCache()
    {
    }

    public void AddNewWebsocket(Guid deviceId, WebSocket webSocket)
    {
        if (sockets.ContainsKey(deviceId))
        {
            sockets[deviceId] = webSocket;
        }
        else
        {
            sockets.Add(deviceId, webSocket);
        }
    }

    public void Remove(Guid deviceId)
    {
        sockets.Remove(deviceId);
    }

    public WebSocket? GetActiveSocket(Guid deviceId)
    {
        return (sockets.ContainsKey(deviceId) && sockets[deviceId].State == WebSocketState.Open) ? sockets[deviceId] : null;
    }
}
