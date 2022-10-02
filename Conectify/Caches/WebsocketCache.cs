namespace Conectify.Server.Caches;

using System.Net.WebSockets;

public interface IWebsocketCache
{
    bool AddNewWebsocket(Guid deviceId, WebSocket webSocket);
    WebSocket? GetActiveSocket(Guid deviceId);
    void Remove(Guid deviceId);

    int GetNoOfActiveSockets(Guid deviceId);
}

public class WebsocketCache : IWebsocketCache
{
    private static readonly Dictionary<Guid, WSCahceItem> sockets = new Dictionary<Guid, WSCahceItem>();
    private readonly object locker = new();

    public WebsocketCache()
    {
    }

    public bool AddNewWebsocket(Guid deviceId, WebSocket webSocket)
    {
        if (sockets.ContainsKey(deviceId))
        {
            lock (locker)
            {
                
                sockets[deviceId].WebSocket = webSocket;
                sockets[deviceId].Count++;
                return false;
            }
        }
        else
        {
            lock (locker)
            {
                sockets.Add(deviceId, new WSCahceItem(webSocket));
                return true;
            }
        }
    }

    public void Remove(Guid deviceId)
    {
        if (sockets.ContainsKey(deviceId))
        {
            if (sockets[deviceId].Count <= 1)
            lock (locker)
            {
                sockets.Remove(deviceId);
                }
            else
            {
                lock (locker)
                {
                    sockets[deviceId].Count--;
                }
            }
        }
    }

    public WebSocket? GetActiveSocket(Guid deviceId)
    {
        return (sockets.ContainsKey(deviceId) && sockets[deviceId].WebSocket.State == WebSocketState.Open) ? sockets[deviceId].WebSocket : null;
    }

    public int GetNoOfActiveSockets(Guid deviceId)
    {
        return sockets.ContainsKey(deviceId) ? sockets[deviceId].Count : 0;
    }
}
