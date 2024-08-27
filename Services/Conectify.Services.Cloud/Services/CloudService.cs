using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Services.Cloud.Services;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;
using System.Text;

namespace Conectify.Services.Cloud.CloudService;

public class CloudService(IServicesWebsocketClient websocketClient, IMapper mapper, DeviceService deviceService, IConnectorService connectorService, CloudConfiguration cloudConfiguration)
{
    private readonly IServicesWebsocketClient websocketClient = websocketClient;

    private readonly Dictionary<Guid, List<IWebsocketBaseModel>> valueCache = [];

    private DateTime LastRefresh = DateTime.MinValue;

    public async Task StartServiceAsync()
    {
        websocketClient.OnIncomingValue += WebsocketClient_OnIncomingValue;
        websocketClient.OnIncomingAction += WebsocketClient_OnIncomingAction;
        websocketClient.OnIncomingCommand += WebsocketClient_OnIncomingCommand;
        await websocketClient.ConnectAsync();

        await RefreshCloudDevices();

        Timer timer = new(CallCloud, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    private async void CallCloud(object? state)
    {
        var finalURL = string.Format("{0}/api/value/", cloudConfiguration.BaseAddress);

        using var client = new HttpClient();
        var message = new HttpRequestMessage(HttpMethod.Get, finalURL);
        var result = await client.SendAsync(message);

        var jsonResult = await result.Content.ReadAsStringAsync();

        var values = JsonConvert.DeserializeObject<ApiValues>(jsonResult);

        foreach(var value in values?.Actuators?? [])
        {
            var action = new WebsocketBaseModel()
            {
                DestinationId = Guid.Parse(value.ActuatorId),
                Name = "cloud",
                NumericValue = value.NumericValue,
                StringValue = value.StringValue,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Unit = "",
                SourceId = cloudConfiguration.SensorId,
                Type = Constants.Types.Action,
            };
           await websocketClient.SendMessageAsync(action);
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


        foreach (var actuatorForCloud  in actuatorsForCloud)
        {
            var finalURL = string.Format("http://{0}/api/actuators", cloudConfiguration.BaseAddress);
            finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");

            var typeMetadata = actuatorForCloud.Metadata.FirstOrDefault(x => x.MetadataId == Constants.Metadatas.IOTypeMetada);
            string ioType = Constants.Metadatas.DefaultIOType;
            if (typeMetadata is not null)
            {
                ioType = typeMetadata.NumericValue.ToString() ?? Constants.Metadatas.DefaultIOType;
            }

            var lastValue = await connectorService.LoadLastValue(actuatorForCloud.SensorId);

            var apiCloudActuator = new ApiCloudActuator(actuatorForCloud.Id.ToString(), actuatorForCloud.Name, lastValue?.StringValue ?? string.Empty, lastValue?.NumericValue ?? 0f ,lastValue?.Unit ?? String.Empty, ioType);

            using var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Post, finalURL)
            {
                Content = new StringContent(JsonConvert.SerializeObject(apiCloudActuator), Encoding.UTF8, "application/json")
            };
            var result = await client.SendAsync(message);
        }
    }

    private void WebsocketClient_OnIncomingCommand(Command command)
    {
        var value = mapper.Map<WebsocketBaseModel>(command);
        var targetDevice = deviceService.GetDeviceById(value.SourceId);

        if (targetDevice is not null)
        {
            if (valueCache.TryGetValue(targetDevice.Value, out var values))
            {
                values.Add(value);
            }
        }
    }

    private void WebsocketClient_OnIncomingAction(Database.Models.Values.Action action)
    {
        
    }

    private async void WebsocketClient_OnIncomingValue(Value value)
    {
        await RefreshCloudDevices();
    }

    private record ApiCloudActuator(string ActuatorId, string ActuatorName, string StringValue, float NumericValue, string Unit, string ActuatorType);

    private record ApiValue(string ActuatorId, string StringValue, float NumericValue);

    private record ApiValues(ApiValue[] Actuators);
}
