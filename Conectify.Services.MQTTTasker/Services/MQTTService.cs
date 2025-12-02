using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using MQTTnet.Server;
using System.Threading;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Conectify.Services.Library;
using System.Reflection.Metadata.Ecma335;
using Conectify.Database.Interfaces;
using Newtonsoft.Json;
using Conectify.Services.MQTTTasker.Models;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Library;

namespace Conectify.Services.MQTTTasker.Services;

public class MqttService : BackgroundService
{
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;
    private readonly Configuration configuration;
    private readonly IConnectorService connectorService;
    private readonly IServicesWebsocketClient websocketClient;
    private readonly ILogger<MqttService> logger;
    private Dictionary<string, Guid> knownDevices = [];

    public MqttService(Configuration configuration, IConnectorService connectorService, IServicesWebsocketClient websocketClient, ILogger<MqttService> logger)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithClientId("ZigbeeWebApi")
            .WithTcpServer(configuration.Broker, 1883)
            .Build();

        _client.ConnectedAsync += async e =>
        {
            await _client.SubscribeAsync("zigbee2mqtt/#");
            logger.LogInformation("📡 Subscribed to zigbee2mqtt/#");
        };

        _ = _client.ConnectAsync(_options).Result;

        var applicationMessage = new MqttApplicationMessageBuilder()
    .WithTopic("zigbee2mqtt/bridge/query")
    .WithPayload("{\"what\": \"devices\"}")
        .Build();

        _client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);


            logger.LogInformation($"[{topic}] {payload}");

            await DecompileMessageAsync(topic, payload);
        };

        var result = _client.PublishAsync(applicationMessage).Result;
        this.configuration = configuration;
        this.connectorService = connectorService;
        this.websocketClient = websocketClient;
        this.logger = logger;
    }

    private async Task DecompileMessageAsync(string topic, string payload)
    {
        if(topic == "zigbee2mqtt/bridge/info")
        {
            RegisterAllDevices(payload);
        }
        else
        {
            await DecodeIncomingValueAsync(topic, payload);
        }
    }

    private async Task DecodeIncomingValueAsync(string topic, string payload)
    {
        const string prefix = "zigbee2mqtt/";

        if (!topic.StartsWith(prefix))
            return;

        string deviceName = topic.Substring(prefix.Length);

        if (knownDevices.TryGetValue(deviceName, out Guid id))
        {
            await SendValueFromDoorSensor(id, payload);
        }
    }

    private async Task SendValueFromDoorSensor(Guid id, string payload)
    {
        var value = JsonConvert.DeserializeObject< IkeaDoorSensorValue >(payload);

        if (value is not null)
        {
            await websocketClient.SendMessageAsync(new WebsocketEvent()
            {
                Name = "Doors",
                NumericValue = value.contact ? 1 : 0,
                StringValue = value.contact ? "Closed" : "Open",
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Type = Constants.Events.Value,
                SourceId = id
            });
            //await websocketClient.SendMessageAsync(new WebsocketEvent()
            //{
            //    Name = "Battery",
            //    NumericValue = value.battery,
            //    TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            //    Type = Constants.Events.Value,
            //    SourceId = id,
            //    Unit = "%"
                
            //});
        }

    }

    private void RegisterAllDevices(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        List<ApiSensor> sensors = [];
        if (root.TryGetProperty("config", out var configObj) &&
            configObj.TryGetProperty("devices", out var devicesObj))
        {
            foreach (var device in devicesObj.EnumerateObject())
            {
                var id = GetGuidFromID(device.Name);
                string name = device.Value.GetProperty("friendly_name").GetString();
                if (name is null) continue;
                sensors.Add(new ApiSensor()
                {
                    Id = id,
                    Name = name,
                    SourceDeviceId = configuration.DeviceId,
                });

                knownDevices.TryAdd(name, id);
            }
            connectorService.RegisterSensors(sensors);
        }
    }

    private Guid GetGuidFromID(string ZigBeeId)
    {
        // Remove "0x" prefix
        string hex = ZigBeeId.StartsWith("0x") ? ZigBeeId.Substring(2) : ZigBeeId;

        // Repeat it twice to make 32 characters
        string hexForGuid = hex + hex;

        // Parse as GUID
        return Guid.ParseExact(hexForGuid, "N");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_client.IsConnected)
            {
                try
                {
                    await _client.ConnectAsync(_options, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError($"MQTT connect error: {ex.Message}");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            await Task.Delay(1000, stoppingToken);
        }

    }
}
