using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Services.Cloud.Services;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;
using System.Text;

namespace Conectify.Services.Cloud.CloudService;

public class CloudService(IServicesWebsocketClient websocketClient, IMapper mapper, DeviceService deviceService, IConnectorService connectorService, CloudConfiguration cloudConfiguration)
{
    private readonly IServicesWebsocketClient websocketClient = websocketClient;

    private readonly Dictionary<Guid, List<IWebsocketBaseModel>> valueCache = [];

    public void StartServiceAsync()
    {
        websocketClient.OnIncomingValue += WebsocketClient_OnIncomingValue;
        websocketClient.OnIncomingAction += WebsocketClient_OnIncomingAction;
        websocketClient.OnIncomingCommand += WebsocketClient_OnIncomingCommand;
        websocketClient.ConnectAsync();

        RefreshCloudDevices().RunSynchronously();
    }

    private async Task RefreshCloudDevices()
    {
        var actuators = await connectorService.LoadAllActuators();
        var cloudActuators = actuators.Where(a => a.Metadata.Any(m => m.Id == Guid.Parse("fd247417-9c50-4108-a8ad-f4899268c706"))).ToList();
    
    
        foreach(var cloudActuator  in cloudActuators)
        {
            var finalURL = string.Format("{0}/api/actuators", cloudConfiguration.BaseAddress);
            finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");

            var serializedApiModel = JsonConvert.SerializeObject(new ApiCloudActuator(cloudActuator.Id.ToString(), cloudActuator.Name, "", 0 ,"", "1"));

            using var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Post, finalURL)
            {
                Content = new StringContent(serializedApiModel, Encoding.UTF8, "application/json")
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

    private void WebsocketClient_OnIncomingValue(Value value)
    {
        
    }

    private record ApiCloudActuator(string ActuatorId, string ActuatorName, string StringValue, float NumericValue, string Unit, string ActuatorType);
}
