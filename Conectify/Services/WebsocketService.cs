namespace Conectify.Server.Services;

using System.Net.WebSockets;
using System.Text;
using Conectify.Server.Caches;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Interfaces;

public interface IWebSocketService
{
    Task<bool> ConnectAsync(Guid thingId, WebSocket webSocket, CancellationToken ct = default);
    Task<bool> TestConnectionAsync(string testMessage, WebSocket webSocket, CancellationToken ct = default);
    Task<bool> SendToDeviceAsync(Guid thingId, IWebsocketModel returnValue, CancellationToken cancelationToken = default);
    Task<bool> SendToDeviceAsync(Guid thingId, string rawString, CancellationToken cancelationToken = default);
    Task<bool> DirectInsert(Guid deviceId, string message, CancellationToken ct = default);
}

public class WebSocketService(ILogger<WebSocketService> logger, ISubscribersCache cache, IServiceProvider serviceProvider, IDeviceService deviceService, IWebsocketCache websocketCache) : IWebSocketService
{
    private const int MaxMessageSize = 1024 * 1024;

    /// <summary>
    /// How long a device gets to take a message. A device that does not take it by then has stopped reading; waiting
    /// for it would hold up the connection whose event is being delivered, so its connection is dropped instead.
    /// </summary>
    public TimeSpan SendTimeout { get; init; } = TimeSpan.FromSeconds(10);

    public async Task<bool> ConnectAsync(Guid deviceId, WebSocket webSocket, CancellationToken ct = default)
    {
        websocketCache.AddNewWebsocket(deviceId, webSocket);
        try
        {
            await deviceService.TryAddUnknownDevice(deviceId, ct: ct);
            await cache.UpdateSubscriber(deviceId, ct);
            logger.LogInformation("Connection with device {deviceId} has started.", deviceId);
            await HandleInput(webSocket, deviceId, ct);
        }
        catch (Exception ex)
        {
            // A device that loses power or wifi does not say goodbye, its connection just breaks.
            logger.LogWarning("Connection with device {deviceId} broke: {message}", deviceId, ex.Message);
        }
        finally
        {
            logger.LogWarning("Connection with device {deviceId} has ended.", deviceId);
            await websocketCache.Remove(deviceId, webSocket, CancellationToken.None);
            if (websocketCache.GetNoOfActiveSockets(deviceId) < 1)
            {
                cache.RemoveSubscriber(deviceId);
            }
            else
            {
                logger.LogWarning("There is another websocket connected as {deviceId}, traffic goes there", deviceId);
            }
        }

        return true;
    }

    private async Task HandleInput(WebSocket webSocket, Guid deviceId, CancellationToken ct)
    {
        var buffer = new byte[1024 * 4];
        using var message = new MemoryStream();
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            // A message does not have to come in one piece, only the whole of it is the JSON.
            message.Write(buffer, 0, result.Count);
            if (message.Length > MaxMessageSize)
            {
                logger.LogError("Message from {deviceId} is bigger than {MaxMessageSize} bytes, closing the connection", deviceId, MaxMessageSize);
                break;
            }

            if (!result.EndOfMessage)
            {
                continue;
            }

            var incomingJson = Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
            message.SetLength(0);
            logger.LogInformation("{incomingJson}", incomingJson);
            var dataService = serviceProvider.GetRequiredService<IDataService>();
            await dataService.InsertJsonModel(incomingJson, deviceId, ct);
        }
        logger.LogCritical("Websocket have been closed! Websocket state: {State}, client: {deviceId}", webSocket.State, deviceId);
    }

    public async Task<bool> SendToDeviceAsync(Guid deviceId, IWebsocketModel returnValue, CancellationToken cancelationToken = default)
    {
        return await Tracing.Trace(async () =>
        {
            return await SendToDeviceAsync(deviceId, returnValue.ToJson(), cancelationToken);
        }, returnValue.Id, $"Sending to {deviceId}");
    }

    public async Task<bool> SendToDeviceAsync(Guid deviceId, string rawString, CancellationToken cancelationToken = default)
    {
        var socket = websocketCache.GetActiveSocket(deviceId);
        if (socket is null)
        {
            cache.RemoveSubscriber(deviceId);
            logger.LogError("Cannot send message to {deviceId}", deviceId);
            logger.LogError("{message}", "Socket does not exist");
            return false;
        }

        try
        {
            var msg = Encoding.UTF8.GetBytes(rawString);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancelationToken);
            timeout.CancelAfter(SendTimeout);

            await socket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, timeout.Token);
            logger.LogInformation("Value sended to active websocket {deviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            // The connection is broken or the device stopped reading. Dropping it ends its receive loop, which cleans
            // it up - and the device, if it is still alive, connects again.
            socket.Abort();
            logger.LogError("Cannot send message to {deviceId}", deviceId);
            logger.LogError("{message}", ex.Message);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(string testMessage, WebSocket webSocket, CancellationToken ct = default)
    {
        int i = 0;
        logger.LogInformation("TestConnection opened: message={TestMessage}", testMessage);
        do
        {
            var msg = Encoding.UTF8.GetBytes(i++.ToString() + " " + testMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, ct);

        } while (webSocket.State == WebSocketState.Open);

        logger.LogInformation("TestConnection closed after {Count} messages", i);
        return true;
    }

    public async Task<bool> DirectInsert(Guid deviceId, string message, CancellationToken ct = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        await dataService.InsertJsonModel(message, deviceId, ct);
        return true;

    }
}
