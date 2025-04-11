namespace Conectify.Server.Services;

using Conectify.Server.Caches;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Interfaces;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

public interface IWebSocketService
{
    Task<bool> ConnectAsync(Guid thingId, WebSocket webSocket, CancellationToken ct = default);
    Task<bool> TestConnectionAsync(string testMessage, WebSocket webSocket, CancellationToken ct = default);
    Task<bool> SendToDeviceAsync(Guid thingId, IWebsocketModel returnValue, CancellationToken cancelationToken = default);
    Task<bool> SendToDeviceAsync(Guid thingId, string rawString, CancellationToken cancelationToken = default);
}

public class WebSocketService(ILogger<WebSocketService> logger, ISubscribersCache cache, IServiceProvider serviceProvider, IDeviceService deviceService, IWebsocketCache websocketCache) : IWebSocketService
{
    public async Task<bool> ConnectAsync(Guid deviceId, WebSocket webSocket, CancellationToken ct = default)
    {
        websocketCache.AddNewWebsocket(deviceId, webSocket);
        await deviceService.TryAddUnknownDevice(deviceId, ct: ct); ;
        await cache.UpdateSubscriber(deviceId, ct);
        logger.LogInformation("Connection with device {deviceId} has started.", deviceId);
        await HandleInput(webSocket, deviceId, ct);

        logger.LogWarning("Connection with device {deviceId} has ended.", deviceId);
        await websocketCache.Remove(deviceId, ct);
        if (websocketCache.GetNoOfActiveSockets(deviceId) < 1)
        {
            cache.RemoveSubscriber(deviceId);
        }
        else
        {
            logger.LogWarning("There was already websocket connected as {deviceId} so I have redirected old traffic to new one", deviceId);
        }

        return true;
    }

    private async Task HandleInput(WebSocket webSocket, Guid deviceId, CancellationToken ct)
    {
        do
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

            if (result.CloseStatus.HasValue)
            {
                break;
            }
            else
            {
                var incomingJson = Encoding.UTF8.GetString(buffer);
                logger.LogInformation("{incomingJson}",incomingJson);
                var dataService = serviceProvider.GetRequiredService<IDataService>();
                await dataService.InsertJsonModel(incomingJson, deviceId, ct);
            }

        } while (webSocket.State == WebSocketState.Open);
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
        try
        {
            var msg = Encoding.UTF8.GetBytes(rawString);
            var socket = websocketCache.GetActiveSocket(deviceId) ?? throw new InvalidOperationException("Socket does not exist");

            await socket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, cancelationToken);
            logger.LogInformation("Value sended to active websocket {deviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            cache.RemoveSubscriber(deviceId);
            await websocketCache.Remove(deviceId, cancelationToken);
            logger.LogError("Cannot send message to {deviceId}", deviceId);
            logger.LogError("{message}",ex.Message);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(string testMessage, WebSocket webSocket, CancellationToken ct = default)
    {
        int i = 0;
        Console.WriteLine("Test connection opened!");
        do
        {
            var msg = Encoding.UTF8.GetBytes(i++.ToString() + " " + testMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, ct);

        } while (webSocket.State == WebSocketState.Open);

        Console.WriteLine("Test connection closed!");
        return true;
    }
}
