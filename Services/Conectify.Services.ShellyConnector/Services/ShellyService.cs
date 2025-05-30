﻿using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;

namespace Conectify.Services.ShellyConnector.Services;

public interface IShellyService
{
    Task<bool> SetSwitch(Guid id, bool isOn, CancellationToken cancellationToken = default);
    Task<bool> Trigger(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SendValueToShelly(Database.Models.Values.Action websocketAction, CancellationToken cancellationToken = default);
    Task<bool> LongPress(Guid id, CancellationToken cancellationToken = default);
}

public class ShellyService(Configuration configuration, IServicesWebsocketClient websocketClient, ILogger<ShellyService> logger) : IShellyService
{
    public async Task<bool> LongPress(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation($"Registered long press");

        var value = new WebsocketBaseModel()
        {
            Name = "Press",
            NumericValue = configuration.LongPressDefaultValue,
            SourceId = configuration.LongPressSensorId,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Unit = "",
            Type = Constants.Events.Value,
        };

        await websocketClient.SendMessageAsync(value, cancellationToken);

        return true;
    }

    public async Task<bool> SetSwitch(Guid id, bool isOn, CancellationToken cancellationToken = default)
    {
        logger.LogInformation($"Light was turned {LightState(isOn)}");
        var value = new WebsocketBaseModel()
        {
            Name = "Light",
            NumericValue = isOn ? 100 : 0,
            SourceId = configuration.SensorId,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Unit = "%",
            Type = Constants.Events.Value,
        };

        await websocketClient.SendMessageAsync(value, cancellationToken);

        return true;
    }

    private string LightState(bool isOn)
    {
        return isOn ? "on" : "off";
    }

    public async Task<bool> SendValueToShelly(Database.Models.Values.Action websocketAction, CancellationToken cancellationToken = default)
    {
        if (websocketAction.DestinationId != configuration.ActuatorId)
        {
            return false;
        }

        if (websocketAction.NumericValue > 0)
        {
            using (var client = new HttpClient())
            {
                logger.LogInformation("Turning on light");
                logger.LogInformation($"Calling address: {configuration.ShellyIp}/relay/0?turn=on");
                var c = await client.GetAsync($"{configuration.ShellyIp}/relay/0?turn=on", cancellationToken);
                logger.LogInformation(await c.Content.ReadAsStringAsync());
            }
        }
        else
        {
            using (var client = new HttpClient())
            {
                logger.LogInformation("Turning off light");
                logger.LogInformation($"Calling address: {configuration.ShellyIp}/relay/0?turn=off");
                var c = await client.GetAsync($"{configuration.ShellyIp}/relay/0?turn=off", cancellationToken);
                logger.LogInformation(await c.Content.ReadAsStringAsync());
            }
        }

        await websocketClient.SendMessageAsync(new WebsocketBaseModel()
        {
            Id = Guid.NewGuid(),
            Name = "Shelly light",
            NumericValue = websocketAction.NumericValue > 0 ? 100 : 0,
            SourceId = configuration.ActuatorId,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Type = Constants.Events.ActionResponse,
            Unit = "%",
            ResponseSourceId = websocketAction.Id,
            StringValue = string.Empty
        }, cancellationToken);
        return true;
    }

    public Task<bool> Trigger(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
