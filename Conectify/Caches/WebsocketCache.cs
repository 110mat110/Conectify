namespace Conectify.Server.Caches;

using System.Diagnostics.Metrics;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;

public interface IWebsocketCache
{
    bool AddNewWebsocket(Guid deviceId, WebSocket webSocket);
    WebSocket? GetActiveSocket(Guid deviceId);

    /// <summary>
    /// Closes this one connection of the device and forgets it. Other connections of the same device are left alone -
    /// a device that reconnected before its old connection was noticed as dead must keep the new one.
    /// </summary>
    Task Remove(Guid deviceId, WebSocket webSocket, CancellationToken cancellationToken);

    int GetNoOfActiveSockets(Guid deviceId);

    public bool IsActiveSocket(Guid deviceId);

    public int ActiveSocketCount { get; }
}

public class WebsocketCache(IServiceProvider serviceProvider, ILogger<WebsocketCache> logger) : IWebsocketCache
{
    private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);

    // A device can be connected more than once: a reconnect arrives before the old connection is noticed as dead,
    // and the UI connects as the same user from every tab. Newest connection is last and gets the traffic.
    private readonly Dictionary<Guid, List<WebSocket>> sockets = [];
    private readonly object locker = new();

    public int ActiveSocketCount
    {
        get
        {
            lock (locker)
            {
                return sockets.Count;
            }
        }
    }

    public bool AddNewWebsocket(Guid deviceId, WebSocket webSocket)
    {
        lock (locker)
        {
            if (sockets.TryGetValue(deviceId, out var deviceSockets))
            {
                deviceSockets.Add(webSocket);
                return false;
            }

            sockets.Add(deviceId, [webSocket]);
        }

        CountConnection(1);
        return true;
    }

    public async Task Remove(Guid deviceId, WebSocket webSocket, CancellationToken cancellationToken)
    {
        var lastConnection = false;
        lock (locker)
        {
            if (sockets.TryGetValue(deviceId, out var deviceSockets) && deviceSockets.Remove(webSocket) && deviceSockets.Count == 0)
            {
                sockets.Remove(deviceId);
                lastConnection = true;
            }
        }

        if (lastConnection)
        {
            CountConnection(-1);
        }

        try
        {
            if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                // Only our half of the closing handshake - a connection that is already dead would never answer.
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(CloseTimeout);
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", timeout.Token);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception when removing websocket");
            webSocket.Abort();
        }
        finally
        {
            webSocket.Dispose();
        }
    }

    public WebSocket? GetActiveSocket(Guid deviceId)
    {
        lock (locker)
        {
            return sockets.TryGetValue(deviceId, out var deviceSockets) ? deviceSockets.LastOrDefault(x => x.State == WebSocketState.Open) : null;
        }
    }

    public int GetNoOfActiveSockets(Guid deviceId)
    {
        lock (locker)
        {
            return sockets.TryGetValue(deviceId, out var deviceSockets) ? deviceSockets.Count : 0;
        }
    }

    public bool IsActiveSocket(Guid deviceId)
    {
        lock (locker)
        {
            return sockets.TryGetValue(deviceId, out var deviceSockets) && deviceSockets.Exists(x => x.State == WebSocketState.Open);
        }
    }

    private void CountConnection(int change)
    {
        using var scope = serviceProvider.CreateScope();
        var meterFactory = scope.ServiceProvider.GetService<IMeterFactory>();
        if (meterFactory is not null)
        {
            var meter = meterFactory.Create("CustomMeters");
            var counter = meter.CreateCounter<int>("connections_count");
            counter.Add(change);
        }
    }
}
