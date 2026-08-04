using Conectify.Services.Library;
using Conectify.Services.Shelly.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;
using System.Text;

namespace Conectify.Services.Shelly.Services;

public interface IShellyService
{
    public Task ReceiveMessages(WebSocket webSocket, string? remoteIp = null, CancellationToken cancellationToken = default);
    Task SendValueToShelly(Database.Models.Values.Event action);
}

public class ShellyService(ShellyFactory shellyFactory, WebsocketCache cache, IServicesWebsocketClient websocketClient, ILogger<ShellyService> logger) : IShellyService
{
    private static readonly string[] SupportedEvents = ["double_push", "triple_push", "long_push", "single_push", "btn_down"];

    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    public async Task ReceiveMessages(WebSocket webSocket, string? remoteIp = null, CancellationToken cancellationToken = default)
    {
        // A device without a power meter reports nothing until its state changes. Waiting for it to
        // speak first means that after a restart it only rejoins the system when somebody flips the
        // switch by hand - so ask who it is the moment it connects.
        await SendMessage(webSocket, "Shelly.GetDeviceInfo", null);

        var buffer = new byte[1024 * 4];
        using var payload = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.CloseStatus.HasValue)
            {
                break;
            }

            payload.Write(buffer, 0, result.Count);

            // A status message can outgrow the buffer. Parsing a single frame would hand the
            // deserializer truncated JSON and the whole message would be thrown away.
            if (!result.EndOfMessage)
            {
                continue;
            }

            var message = Encoding.UTF8.GetString(payload.ToArray());
            payload.SetLength(0);

            var sw = Stopwatch.StartNew();
            try
            {
                var deserialized = JsonConvert.DeserializeObject<ShellyWS>(message, SerializerSettings);

                if (deserialized is not null)
                {
                    logger.LogDebug("RECEIVED: {msg}", message);
                    await ReadMessage(deserialized, message, webSocket, remoteIp);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Shelly ws message failed! : {message}", message);
                logger.LogError(ex, "Shelly message read failed");
            }
            finally
            {
                sw.Stop();
                cache.ProcessInboundDuration(sw.Elapsed);
            }
        } while (!result.CloseStatus.HasValue);

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

    }

    private async Task ReadMessage(ShellyWS message, string rawMessage, WebSocket source, string? remoteIp)
    {
        var src = message.Src;

        if (string.IsNullOrEmpty(src))
        {
            return;
        }

        if (cache.Cache.TryGetValue(src, out ShellyDeviceCacheItem? shelly) && shelly.Shelly is not null)
        {
            // Keeps the dashboard's view current (address, firmware, relay and power state) and
            // re-points the entry at the current socket when a device reconnects.
            shelly.Record(message, source, remoteIp);
            await WebsocketStateInput(message, shelly);

            // Shelly.GetStatus answers with the same payload a notification puts under "params",
            // only under "result" - so it needs a second read to be seen as a status at all.
            var status = AsStatus(rawMessage);
            if (status is not null)
            {
                shelly.Record(status, source, remoteIp);
                await WebsocketStateInput(status, shelly);
            }
        }
        else
        {
            await InitializeShelly(message, source, src, remoteIp);
        }
    }

    /// <summary>
    /// Re-reads a reply whose "result" carries a device status. Replies that carry something else
    /// (device info, wifi status) deserialize into an empty status and are ignored downstream.
    /// </summary>
    private static ShellyWS? AsStatus(string rawMessage)
    {
        var reply = JsonConvert.DeserializeObject<ShellyStatusReply>(rawMessage, SerializerSettings);

        return reply?.Result is null
            ? null
            : new ShellyWS { Src = reply.Src, Params = reply.Result };
    }

    private async Task WebsocketStateInput(ShellyWS message, ShellyDeviceCacheItem shelly)
    {
        if (shelly.Shelly is null || message.Params is null)
        {
            return;
        }


        if (message.Params.Switch0?.Output is not null)
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
        }

        if (message.Params.Switch0?.aenergy is not null && shelly.Shelly.Switches[0].Power?.SensorId is not null)
        {
            await SendPower(CalculatePower(message.Params.Switch0?.aenergy?.ByMinute?[0]), shelly.Shelly.Switches[0].Power!.SensorId);
        }

        if (message.Params.Switch1?.Output is not null)
        {
            var value = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
                Name = "Light",
                NumericValue = message.Params.Switch1.Output.Value ? 100 : 0,
                StringValue = "",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Unit = "%",
                SourceId = shelly.Shelly.Switches[1].SensorId,
                Type = Constants.Events.Value,
            };

            await websocketClient.SendMessageAsync(value);
        }

        if (message.Params.Switch1?.aenergy is not null && shelly.Shelly.Switches[1]?.Power?.SensorId is not null)
        {
            await SendPower(CalculatePower(message.Params.Switch1?.aenergy?.ByMinute?[0]), shelly.Shelly.Switches[1].Power!.SensorId);
        }
        if (message.Params.Switch2?.Output is not null)
        {
            var value = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
                Name = "Light",
                NumericValue = message.Params.Switch2.Output.Value ? 100 : 0,
                StringValue = "",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Unit = "%",
                SourceId = shelly.Shelly.Switches[2].SensorId,
                Type = Constants.Events.Value,
            };

            await websocketClient.SendMessageAsync(value);
        }
        else
        if (message.Params.Switch2 is not null)
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

            if (message.Params.Switch2?.aenergy is not null && shelly.Shelly.Switches[2].Power?.SensorId is not null)
            {
                await SendPower(CalculatePower(message.Params.Switch2?.aenergy?.ByMinute?[0]), shelly.Shelly.Switches[2].Power!.SensorId);
            }
        }

        if (message.Params.events is not null && message.Params.events.Length != 0 && SupportedEvents.Contains(message.Params.events[0].@event))
        {
            var input = message.Params.events[0]?.id;

            if (input is null)
            {
                return;
            }

            var evnt = new WebsocketEvent()
            {
                Id = Guid.NewGuid(),
                Name = "Input",
                TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                SourceId = shelly.Shelly.DetachedInputs[input.Value].SensorId,
                Type = message.Params.events[0].@event!,
            };
            await websocketClient.SendMessageAsync(evnt);
        }

        //Powermeter
        if (message.Params.em0 is not null)
        {
            var measuredPower = Math.Abs(message.Params?.em0?.total_act_power ?? 0);
            await SendPower(measuredPower, shelly.Shelly.Powers[0].SensorId);
        }

        if (message?.Params?.Pm0?.apower is not null && shelly.Shelly.Powers[0] is not null)
        {

            var measuredPower = message.Params?.Pm0?.apower;
            await SendPower(measuredPower, shelly.Shelly.Powers[0].SensorId);
        }
    }

    private async Task SendPower(float? pwr, Guid sensorId)
    {
        var res = cache.ProcessFrequentValue(sensorId, pwr, TimeSpan.FromMinutes(1));

        if (res is null)
        {
            return;
        }

        var message = new WebsocketEvent()
        {
            Id = Guid.NewGuid(),
            Name = "Power",
            NumericValue = res,
            StringValue = "",
            TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Unit = "W",
            SourceId = sensorId,
            Type = Constants.Events.Value,
        };

        await websocketClient.SendMessageAsync(message);
    }

    private async Task InitializeShelly(ShellyWS message, WebSocket source, string src, string? remoteIp)
    {
        if (message.Result is not null && !string.IsNullOrEmpty(message.Result.Model))
        {
            var shellyModel = await shellyFactory.GetShelly(message.Result.Model, src, message.Result.Name ?? string.Empty);

            if (shellyModel is null)
            {
                logger.LogWarning("Cannot load shelly {model}", message.Result.Model);
                return;
            }

            if (!cache.Cache.TryAdd(src, new ShellyDeviceCacheItem()
            {
                ShellyId = src,
                WebSocket = source,
                Shelly = shellyModel,
            }))
            {
                cache.Cache[src].Shelly = shellyModel;
            }

            var device = cache.Cache[src];
            device.Record(message, source, remoteIp);
            logger.LogInformation("Updating shelly info scr: {src}", src);

            // Whatever the device announced before we knew what it was would otherwise be dropped,
            // and with it the relay state that makes the light appear in the system.
            if (cache.PendingStatus.TryRemove(src, out var pending))
            {
                device.Record(pending, source, remoteIp);
                await WebsocketStateInput(pending, device);
            }

            // Its current state, so the server hears about the device without anyone touching it.
            // The reply also carries wifi.sta_ip - the address the dashboard links to, which the
            // socket cannot supply because in Docker that is only the NAT address.
            await SendMessage(source, "Shelly.GetStatus", null);
        }
        else
        {
            // Held until the device identifies itself, then replayed above.
            if (message.Params is not null)
            {
                cache.PendingStatus[src] = message;
            }

            await SendMessage(source, "Shelly.GetDeviceInfo", null);
            logger.LogInformation("Requesting shelly {src} for more info", src);
        }
    }

    private async Task SendMessage(string deviceId, string method, object? parameters)
    {
        // Both of these used to return silently, so a command that got this far simply vanished.
        if (!cache.Cache.TryGetValue(deviceId, out ShellyDeviceCacheItem? cacheItem))
        {
            logger.LogWarning("Cannot send {method}: shelly {deviceId} is not in the cache", method, deviceId);
            return;
        }

        if (cacheItem.WebSocket?.State is not WebSocketState.Open)
        {
            logger.LogWarning("Cannot send {method}: socket for shelly {deviceId} is {state}, dropping it from the cache",
                method, deviceId, cacheItem.WebSocket?.State.ToString() ?? "missing");
            cache.Cache.TryRemove(deviceId, out _);
            return;
        }

        await SendMessage(cacheItem.WebSocket, method, parameters);
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
        var sw = Stopwatch.StartNew();
        var shelly = cache.Cache.Values.FirstOrDefault(x => x.Shelly.Switches.Any(x => x.ActuatorId == evnt.DestinationId));
        if (shelly is null)
        {
            // Spell out what the service actually holds - the answer to "is it initialized" is the
            // difference between an empty list here and a list that just does not contain the id.
            var known = cache.Cache.Values
                .SelectMany(device => device.Shelly.Switches
                    .Select(sw => $"{device.ShellyId}/SW-{sw.ShellyId}={sw.ActuatorId}"))
                .ToList();

            logger.LogWarning("Target actuator not found. ID: {id}. Cached devices: {deviceCount}, known actuators: {known}",
                evnt.DestinationId, cache.Cache.Count, known.Count == 0 ? "(none)" : string.Join(", ", known));
            return;
        }

        var target = shelly.Shelly.Switches.First(x => x.ActuatorId == evnt.DestinationId);

        await SendMessage(shelly.ShellyId, "Switch.Set", new { id = target.ShellyId, on = evnt.NumericValue != 0 });

        sw.Stop();
        cache.ProcessOutbandDuration(sw.Elapsed);
    }

    private static float? CalculatePower(float? aenergy)
    {
        return aenergy * 60 / 1000;
    }
}
