using Conectify.Services.Library;
using Conectify.Services.Shelly.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace Conectify.Services.Shelly.Services;

public interface IShellyService
{
    public Task ReceiveMessages(WebSocket webSocket, CancellationToken cancellationToken = default);
    Task SendValueToShelly(Database.Models.Values.Event action);
}

public enum ShellyType
{
    Unknown,
    Shelly1g3,
    Shellyi4,
    Shelly2pmg3,
    ShellyPro3,
}

public class ShellyService(ShellyFactory shellyFactory, WebsocketCache cache, IServicesWebsocketClient websocketClient, ILogger<ShellyService> logger) : IShellyService
{
    public async Task ReceiveMessages(WebSocket webSocket, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                var deserialized = JsonConvert.DeserializeObject<ShellyWS>(message, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });

                if (deserialized is not null)
                {
                    logger.LogInformation("RECEIVED: {msg}", message);
                    await ReadMessage(deserialized, webSocket);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Shelly ws message failed to deserialize! : {message}", message);
            }

            // Write the received message to the console
            logger.LogDebug("Received: {message}", message);

        } while (!result.CloseStatus.HasValue);

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

    }

    private async Task ReadMessage(ShellyWS message, WebSocket source)
    {
        var src = message.Src;

        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        if (cache.Cache.TryGetValue(src, out ShellyDeviceCacheItem? shelly) && shelly.Shelly is not null)
        {
            await WebsocketStateInput(message, shelly);
        }
        else
        {
            await InitializeShelly(message, source, src);
        }
    }

    private async Task WebsocketStateInput(ShellyWS message, ShellyDeviceCacheItem shelly)
    {
        if (message.Params?.Switch0 is not null && shelly.Shelly is not null)
        {
            var value = new WebsocketEvent()
            {
                Name = "Light",
                NumericValue = message.Params.Switch0.on ? 100 : 0,
                StringValue = "",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Unit = "%",
                SourceId = shelly.Shelly.Switches[0].SensorId,
                Type = Constants.Events.Value,
            };

            await websocketClient.SendMessageAsync(value);

            if (message.Params.Switch0.aenergy is not null && shelly.Shelly.Switches[0].Power is not null)
            {
                var pwr = new WebsocketEvent()
                {
                    Name = "Power",
                    NumericValue = message.Params.Switch0.aenergy.ByMinute[0],
                    StringValue = "",
                    TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Unit = "%",
                    SourceId = shelly.Shelly.Switches[0].Power.SensorId,
                    Type = Constants.Events.Value,
                };

                await websocketClient.SendMessageAsync(pwr);
            }
        }
        if (message.Params?.Switch1 is not null && shelly.Shelly is not null)
        {
            var value = new WebsocketEvent()
            {
                Name = "Light",
                NumericValue = message.Params.Switch1.on ? 100 : 0,
                StringValue = "",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Unit = "%",
                SourceId = shelly.Shelly.Switches[1].SensorId,
                Type = Constants.Events.Value,
            };

            await websocketClient.SendMessageAsync(value);

            if (message.Params.Switch1.aenergy is not null && shelly.Shelly.Switches[1]?.Power is not null)
            {
                var pwr = new WebsocketEvent()
                {
                    Name = "Power",
                    NumericValue = message.Params.Switch1.aenergy.ByMinute[0],
                    StringValue = "",
                    TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Unit = "%",
                    SourceId = shelly.Shelly.Switches[1].Power.SensorId,
                    Type = Constants.Events.Value,
                };

                await websocketClient.SendMessageAsync(pwr);
            }
        }

        if (message.Params?.Switch2 is not null && shelly.Shelly is not null)
        {
            var value = new WebsocketEvent()
            {
                Name = "Light",
                NumericValue = message.Params.Switch2.on ? 100 : 0,
                StringValue = "",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Unit = "%",
                SourceId = shelly.Shelly.Switches[2].SensorId,
                Type = Constants.Events.Value,
            };

            await websocketClient.SendMessageAsync(value);

            if (message.Params.Switch2.aenergy is not null && shelly.Shelly.Switches[2].Power is not null)
            {
                var pwr = new WebsocketEvent()
                {
                    Name = "Power",
                    NumericValue = message.Params.Switch2.aenergy.ByMinute[0],
                    StringValue = "",
                    TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Unit = "%",
                    SourceId = shelly.Shelly.Switches[2].Power.SensorId,
                    Type = Constants.Events.Value,
                };

                await websocketClient.SendMessageAsync(pwr);
            }
        }
    }

    private async Task InitializeShelly(ShellyWS message, WebSocket source, string src)
    {
        if (message.Result is not null && !string.IsNullOrEmpty(message.Result.Model))
        {
            var shellyModel = shellyFactory.GetShelly(message.Result.Model, src, message.Result.Name);
            if(!cache.Cache.TryAdd(src, new ShellyDeviceCacheItem()
            {
                ShellyId = src,
                WebSocket = source,
                Shelly = shellyModel,
            }))
            {
                cache.Cache[src].Shelly = shellyModel;
            }
            logger.LogInformation("Updating shelly info scr: {src}", src);

        }
        else
        {
            await SendMessage(source, "Shelly.GetDeviceInfo", null);
            logger.LogInformation("Requesting shelly {src} for more info", src);
        }
    }

    private async Task SendMessage(string deviceId, string method, object? parameters)
    {
        if (cache.Cache.TryGetValue(deviceId, out ShellyDeviceCacheItem? cacheItem))
        {
            if (cacheItem.WebSocket.State != WebSocketState.Open)
            {
                cache.Cache.Remove(deviceId);
                return;
            }

            await SendMessage(cacheItem.WebSocket, method, parameters);
        }
    }

    private async Task SendMessage(WebSocket ws, string method, object? parameters)
    {
            var outboundWs = new OutboundWS()
            {
                method = method,
                Params = parameters,
            };
            var rawString = JsonConvert.SerializeObject(outboundWs);
            var msg = Encoding.UTF8.GetBytes(rawString);

            await ws.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, default);
            logger.LogInformation("SENT TO SHELLY: {msg}", rawString);
    }

    public async Task SendValueToShelly(Database.Models.Values.Event evnt)
    {
        var shelly = cache.Cache.Values.FirstOrDefault(x => x.Shelly.Switches.Any(x => x.ActuatorId == evnt.DestinationId));
        if (shelly is null)
        {
            logger.LogWarning("Target actuator not found. ID: {id}", evnt.DestinationId);
            return;
        }

        var target = shelly.Shelly.Switches.First(x => x.ActuatorId == evnt.DestinationId);

        await SendMessage(shelly.ShellyId, "Switch.Set", new { id = target.ShellyId, on = evnt.NumericValue != 0 });
    }
}
