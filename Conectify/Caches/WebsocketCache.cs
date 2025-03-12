namespace Conectify.Server.Caches;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;

public interface IWebsocketCache
{
    bool AddNewWebsocket(Guid deviceId, WebSocket webSocket);
    WebSocket? GetActiveSocket(Guid deviceId);
    Task Remove(Guid deviceId, CancellationToken cancellationToken);

    int GetNoOfActiveSockets(Guid deviceId);

    public bool IsActiveSocket(Guid deviceId);
}

public class WebsocketCache(IServiceProvider serviceProvider, ILogger<WebsocketCache> logger) : IWebsocketCache
{
    private static readonly Dictionary<Guid, WSCahceItem> sockets = new();
    private readonly object locker = new();

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

    public async Task Remove(Guid deviceId, CancellationToken cancellationToken)
    {
        if (sockets.TryGetValue(deviceId, out WSCahceItem? value))
        {
            var websocket = value.WebSocket;
            try
            {
                await websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", cancellationToken);
                websocket.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception when removing websocket");
            }
            if (value.Count <= 1)
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
        return (sockets.TryGetValue(deviceId, out WSCahceItem? value) && value.WebSocket.State == WebSocketState.Open) ? value.WebSocket : null;
    }



    public int GetNoOfActiveSockets(Guid deviceId)
    {
        return sockets.ContainsKey(deviceId) ? sockets[deviceId].Count : 0;
    }

    public bool IsActiveSocket(Guid deviceId)
    {
        return (sockets.ContainsKey(deviceId) && sockets[deviceId].WebSocket.State == WebSocketState.Open);
    }
}
