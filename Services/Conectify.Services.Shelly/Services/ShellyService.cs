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

public class ShellyService(ShellyFactory shellyFactory, WebsocketCache cache, IServicesWebsocketClient websocketClient, ILogger<ShellyService> logger) : IShellyService
{
    private static readonly string[] SupportedEvents = { "double_push", "triple_push", "long_push", "single_push", "btn_down" };

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
                logger.LogError(ex, "Shelly message read failed");
            }
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
        if (shelly.Shelly is null)
        {
            return;
        }


        if (message.Params?.Switch0?.Output is not null)
        {
            var value = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
                Name = "Light",
                NumericValue = message.Params.Switch0.Output.Value ? 100 : 0,
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
                    Id = Guid.NewGuid(),
                    Name = "Power",
                    NumericValue = CalculatePower(message.Params.Switch0.aenergy.ByMinute[0]),
                    StringValue = "",
                    TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Unit = "W",
                    SourceId = shelly.Shelly.Switches[0].Power.SensorId,
                    Type = Constants.Events.Value,
                };

                await websocketClient.SendMessageAsync(pwr);
            }
        }
        if (message.Params?.Switch1 is not null)
        {
            var value = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
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
                    Id = Guid.NewGuid(),
                    Name = "Power",
                    NumericValue = CalculatePower(message.Params.Switch1.aenergy.ByMinute[0]),
                    StringValue = "",
                    TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Unit = "W",
                    SourceId = shelly.Shelly.Switches[1].Power.SensorId,
                    Type = Constants.Events.Value,
                };

                await websocketClient.SendMessageAsync(pwr);
            }
        }

        if (message.Params?.Switch2 is not null)
        {
            var value = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
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
                if (!cache.FrequentValueCahce.TryGetValue(shelly.Shelly.Powers[0].SensorId, out ShellyFequentValueCahceItem? cacheItem))
                {
                    cacheItem = new ShellyFequentValueCahceItem() { LastSent = DateTime.MinValue };
                    cache.FrequentValueCahce.Add(shelly.Shelly.Powers[0].SensorId, cacheItem);
                }

                var res = cacheItem.ProcessValue(CalculatePower(message.Params.Switch2.aenergy.ByMinute[0]), TimeSpan.FromSeconds(10));

                if (res is null) return;

                var pwr = new WebsocketEvent()
                {
                    Id = Guid.NewGuid(),
                    Name = "Power",
                    NumericValue = res.Value,
                    StringValue = "",
                    TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Unit = "W",
                    SourceId = shelly.Shelly.Switches[2].Power.SensorId,
                    Type = Constants.Events.Value,
                };

                await websocketClient.SendMessageAsync(pwr);
            }
        }

        if (message.Params?.events is not null && message.Params?.events.Length != 0 && SupportedEvents.Contains(message.Params?.events[0].@event))
        {
            var input = message.Params?.events[0]?.id;

            if(input is null)
            {
                return;
            }
            
            var evnt = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
                Name = "Input",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                SourceId = shelly.Shelly.DetachedInputs[input.Value].SensorId,
                Type = message.Params?.events[0].@event!,
            };
            await websocketClient.SendMessageAsync(evnt);
        }

        if (message.Params?.Pm0?.apower is not null && shelly.Shelly.Powers[0] is not null)
        {
            if (!cache.FrequentValueCahce.TryGetValue(shelly.Shelly.Powers[0].SensorId, out ShellyFequentValueCahceItem? value))
            {
                value = new ShellyFequentValueCahceItem() { LastSent = DateTime.MinValue };
                cache.FrequentValueCahce.Add(shelly.Shelly.Powers[0].SensorId, value);
            }

            var res = value.ProcessValue(message.Params?.Pm0?.apower, TimeSpan.FromSeconds(10));

            if (res is null)
            {
                return;
            }

            var pwr = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
                Name = "Power",
                NumericValue = res.Value,
                StringValue = "",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Unit = "W",
                SourceId = shelly.Shelly.Powers[0].SensorId,
                Type = Constants.Events.Value,
            };

            await websocketClient.SendMessageAsync(pwr);
        }
    }

    private async Task InitializeShelly(ShellyWS message, WebSocket source, string src)
    {
        if (message.Result is not null && !string.IsNullOrEmpty(message.Result.Model))
        {
            var shellyModel = await shellyFactory.GetShelly(message.Result.Model, src, message.Result.Name);
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

    private static float CalculatePower(float aenergy)
    {
        return aenergy * 60 / 1000;
    }
}
