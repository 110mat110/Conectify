namespace Conectify.Server.Caches;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using System.Diagnostics.Metrics;
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
    private readonly IServiceProvider serviceProvider;

    public WebsocketCache(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
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
            using var scope = serviceProvider.CreateScope();
            var meterFactory = scope.ServiceProvider.GetService<IMeterFactory>();
            if (meterFactory is not null)
            {
                var meter = meterFactory.Create("CustomMeters");
                var counter = meter.CreateCounter<int>("connections_count");
                counter.Add(1);
            }
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
            {
                using var scope = serviceProvider.CreateScope();
                var meterFactory = scope.ServiceProvider.GetService<IMeterFactory>();
                if (meterFactory is not null)
                {
                    var meter = meterFactory.Create("CustomMeters");
                    var counter = meter.CreateCounter<int>("connections_count");
                    counter.Add(-1);
                }

                lock (locker)
                {
                    sockets.Remove(deviceId);
                }
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
