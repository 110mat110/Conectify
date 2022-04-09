namespace Conectify.Server.Services;

using Conectify.Server.Caches;
using Conectify.Shared.Library.Models.Values;
using System.Net.WebSockets;
using System.Text;

public interface IWebSocketService
{
    Task<bool> ConnectAsync(Guid thingId, WebSocket webSocket, CancellationToken ct = default);
    Task<bool> SendToThingAsync(Guid thingId, IApiBaseModel returnValue, CancellationToken cancelationToken = default);
}

public class WebSocketService : IWebSocketService
{

    private readonly ILogger<WebSocketService> logger;
    private readonly ISubscribersCache cache;
    private readonly IServiceProvider serviceProvider;
    private Dictionary<Guid, WebSocket> sockets = new Dictionary<Guid, WebSocket>();

    public WebSocketService(ILogger<WebSocketService> logger, ISubscribersCache cache, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.cache = cache;
        this.serviceProvider = serviceProvider;
    }

    public async Task<bool> ConnectAsync(Guid deviceId, WebSocket webSocket, CancellationToken ct = default)
    {
        if (sockets.ContainsKey(deviceId))
        {
            sockets[deviceId] = webSocket;
        }
        else
        {
            sockets.Add(deviceId, webSocket);
        }

        var deviceService = serviceProvider.GetRequiredService<IDeviceService>();

        await deviceService.AddUnknownDevice(deviceId, ct);
        await cache.AddSubscriber(deviceId, ct);
        await HandleInput(webSocket, deviceId, ct);

        logger.LogWarning($"Connection with device {deviceId} has ended.");
        sockets.Remove(deviceId);
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
        var msg = Encoding.UTF8.GetBytes(returnValue.ToJson());

        if (sockets.ContainsKey(thingId) && sockets[thingId].State == WebSocketState.Open)
        {
            await sockets[thingId].SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, cancelationToken);
            return true;
        }
        else
        {
            cache.RemoveSubscriber(thingId);
            sockets.Remove(thingId);
            logger.LogError($"Cannot send message to {thingId}");
            return false;
        }
    }
}
