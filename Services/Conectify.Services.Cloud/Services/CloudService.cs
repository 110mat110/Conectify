using System.Text;
using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;

namespace Conectify.Services.Cloud.Services;

public class CloudService(IServicesWebsocketClient websocketClient, IMapper mapper, DeviceService deviceService, IConnectorService connectorService, CloudConfiguration cloudConfiguration, ILogger<CloudService> logger)
{
    private readonly IServicesWebsocketClient websocketClient = websocketClient;

    private readonly Dictionary<Guid, List<IWebsocketEvent>> valueCache = [];

    private DateTime LastRefresh = DateTime.MinValue;

    public async Task StartServiceAsync()
    {
        logger.LogInformation("CloudService starting, base={BaseAddress}", cloudConfiguration.BaseAddress);
        websocketClient.OnIncomingEvent += WebsocketClient_OnIncomingEvent; ;
        await websocketClient.ConnectAsync();

        await RefreshCloudDevices();

        Timer timer = new(CallCloud, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        logger.LogInformation("CloudService started, polling cloud every 1 minute");
    }

    private void WebsocketClient_OnIncomingEvent(Event evnt)
    {
        if (evnt.Type == Constants.Events.Command)
        {
            WebsocketClient_OnIncomingCommand(evnt);
        }
    }

    private async void CallCloud(object? state)
    {
        var finalURL = string.Format("{0}/api/value/", cloudConfiguration.BaseAddress);
        logger.LogDebug("CallCloud: polling {Url}", finalURL);

        try
        {
            using var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, finalURL);
            var result = await client.SendAsync(message);

            if (!result.IsSuccessStatusCode)
            {
                logger.LogWarning("CallCloud: cloud returned {StatusCode}", result.StatusCode);
                return;
            }

            var jsonResult = await result.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<ApiValues>(jsonResult);
            var actuators = values?.Actuators ?? [];

            logger.LogInformation("CallCloud: received {Count} actuator value(s) from cloud", actuators.Length);

            foreach (var value in actuators)
            {
                logger.LogInformation("CallCloud: sending cloud action to actuator={ActuatorId} numericValue={NumericValue} stringValue={StringValue}",
                    value.ActuatorId, value.NumericValue, value.StringValue);
                var action = new WebsocketEvent()
                {
                    DestinationId = Guid.Parse(value.ActuatorId),
                    Name = "cloud",
                    NumericValue = value.NumericValue,
                    StringValue = value.StringValue,
                    TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Unit = "",
                    SourceId = cloudConfiguration.SensorId,
                    Type = Constants.Events.Action,
                };
                await websocketClient.SendMessageAsync(action);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CallCloud: failed to poll cloud at {Url}", finalURL);
        }
    }

    private async Task RefreshCloudDevices()
    {
        if (DateTime.UtcNow.Subtract(LastRefresh).TotalMinutes < 3)
        {
            return;
        }
        LastRefresh = DateTime.UtcNow;

        var actuators = await connectorService.LoadAllActuators();
        var actuatorsForCloud = actuators.Where(a => a.Metadata.Any(m => m.MetadataId == Constants.Metadatas.CloudMetadata)).ToList();
        logger.LogInformation("RefreshCloudDevices: {Total} actuators total, {CloudCount} marked for cloud", actuators.Count(), actuatorsForCloud.Count);


        foreach (var actuatorForCloud in actuatorsForCloud)
        {
            var finalURL = string.Format("{0}/api/actuators", cloudConfiguration.BaseAddress); ;

            var typeMetadata = actuatorForCloud.Metadata.FirstOrDefault(x => x.MetadataId == Constants.Metadatas.IOTypeMetada);
            string ioType = Constants.Metadatas.DefaultIOType;
            if (typeMetadata is not null)
            {
                ioType = typeMetadata.NumericValue.ToString() ?? Constants.Metadatas.DefaultIOType;
            }

            var lastValue = await connectorService.LoadLastValue(actuatorForCloud.SensorId);

            var apiCloudActuator = new ApiCloudActuator(actuatorForCloud.Id.ToString(), actuatorForCloud.Name, lastValue?.StringValue ?? string.Empty, lastValue?.NumericValue ?? 0f, lastValue?.Unit ?? string.Empty, ioType);

            using var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Post, finalURL)
            {
                Content = new StringContent(JsonConvert.SerializeObject(apiCloudActuator), Encoding.UTF8, "application/json")
            };
            var result = await client.SendAsync(message);
        }
    }

    private void WebsocketClient_OnIncomingCommand(Event command)
    {
        var value = mapper.Map<WebsocketEvent>(command);
        var targetDevice = deviceService.GetDeviceById(value.SourceId);

        if (targetDevice is not null)
        {
            logger.LogInformation("OnIncomingCommand: caching command for device={DeviceId} sourceId={SourceId}", targetDevice.Value, value.SourceId);
            if (valueCache.TryGetValue(targetDevice.Value, out var values))
            {
                values.Add(value);
            }
        }
        else
        {
            logger.LogWarning("OnIncomingCommand: no device found for sourceId={SourceId}", value.SourceId);
        }
    }

    private record ApiCloudActuator(string ActuatorId, string ActuatorName, string StringValue, float NumericValue, string Unit, string ActuatorType);

    private record ApiValue(string ActuatorId, string StringValue, float NumericValue);

    private record ApiValues(ApiValue[] Actuators);
}
