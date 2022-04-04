namespace Conectify.Server.Services;

using Conectify.Shared.Library.Models;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

public interface IWebSocketService
{
    Task<bool> ConnectAsync(Guid thingId, WebSocket webSocket);
    Task<bool> SendToThingAsync(Guid thingId, ApiValueModel returnValue, CancellationToken cancelationToken = default);
}

public class WebSocketService : IWebSocketService
{

    private readonly ILogger<WebSocketService> logger;
    private readonly IDataService dataService;
    private Dictionary<Guid, WebSocket> sockets = new Dictionary<Guid, WebSocket>();

    public WebSocketService(ILogger<WebSocketService> logger, IDataService dataService)
    {
        this.logger = logger;
        this.dataService = dataService;
    }

    public async Task<bool> ConnectAsync(Guid thingId, WebSocket webSocket)
    {
        if (sockets.ContainsKey(thingId))
        {
            sockets[thingId] = webSocket;
        }
        else
        {
            sockets.Add(thingId, webSocket);
        }

        await HandleInput(webSocket);
        return true;
    }

    private async Task HandleInput(WebSocket webSocket)
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

                var inputValue = JsonConvert.DeserializeObject<ApiValueModel>(incomingJson);
                await dataService.InsertApiValue(inputValue);
            }

        } while (true);
    }

    public async Task<bool> SendToThingAsync(Guid thingId, ApiValueModel returnValue, CancellationToken cancelationToken = default)
    {
        var msg = Encoding.UTF8.GetBytes(returnValue.ToJson());

        if (sockets.ContainsKey(thingId) && sockets[thingId].State == WebSocketState.Open)
        {
            await sockets[thingId].SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, cancelationToken);
            return true;
        }
        else
        {
            logger.LogError($"Cannot send message to {thingId}");
            return false;
        }
    }
}
