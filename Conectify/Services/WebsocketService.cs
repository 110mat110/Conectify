namespace Conectify.Server.Services;

using Conectify.Server.Caches;
using Conectify.Shared.Library.Models.Values;
using System.Net.WebSockets;
using System.Text;

public interface IWebSocketService
{
    Task<bool> ConnectAsync(Guid thingId, WebSocket webSocket, CancellationToken ct = default);
    Task<bool> TestConnectionAsync(string testMessage, WebSocket webSocket, CancellationToken ct = default);
    Task<bool> SendToThingAsync(Guid thingId, IApiBaseModel returnValue, CancellationToken cancelationToken = default);
    Task<bool> SendToThingAsync(Guid thingId, string rawString, CancellationToken cancelationToken = default);
}

public class WebSocketService : IWebSocketService
{

    private readonly ILogger<WebSocketService> logger;
    private readonly ISubscribersCache cache;
    private readonly IServiceProvider serviceProvider;
    private readonly IDeviceService deviceService;
    private readonly IWebsocketCache websocketCache;

    public WebSocketService(ILogger<WebSocketService> logger, ISubscribersCache cache, IServiceProvider serviceProvider, IDeviceService deviceService, IWebsocketCache websocketCache)
    {
        this.logger = logger;
        this.cache = cache;
        this.serviceProvider = serviceProvider;
        this.deviceService = deviceService;
        this.websocketCache = websocketCache;
    }

    public async Task<bool> ConnectAsync(Guid deviceId, WebSocket webSocket, CancellationToken ct = default)
    {
        websocketCache.AddNewWebsocket(deviceId, webSocket);
        await deviceService.TryAddUnknownDevice(deviceId, ct: ct); ;
        await cache.AddSubscriber(deviceId, ct);
        await HandleInput(webSocket, deviceId, ct);

        logger.LogWarning($"Connection with device {deviceId} has ended.");
        websocketCache.Remove(deviceId);
        cache.RemoveSubscriber(deviceId);
        return true;
    }

    private async Task HandleInput(WebSocket webSocket, Guid deviceId, CancellationToken ct)
    {
        do
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.CloseStatus.HasValue)
            {
                break;
            }
            else
            {
                var incomingJson = Encoding.UTF8.GetString(buffer);
                logger.LogInformation(incomingJson);
                var dataService = serviceProvider.GetRequiredService<IDataService>();
                await dataService.InsertJsonModel(incomingJson, deviceId, ct);
            }

        } while (true);
    }

    public async Task<bool> SendToThingAsync(Guid thingId, IApiBaseModel returnValue, CancellationToken cancelationToken = default)
    {
        return await SendToThingAsync(thingId, returnValue.ToJson(), cancelationToken);
    }

    public async Task<bool> SendToThingAsync(Guid thingId, string rawString, CancellationToken cancelationToken = default)
    {
        var msg = Encoding.UTF8.GetBytes(rawString);
        var socket = websocketCache.GetActiveSocket(thingId);
        if (socket is not null)
        {
            await socket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, cancelationToken);
            return true;
        }
        cache.RemoveSubscriber(thingId);
        websocketCache.Remove(thingId);
        logger.LogError($"Cannot send message to {thingId}");
        return false;
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
