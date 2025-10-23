using Conectify.Database;
using Conectify.Database.Models.SmartThings;
using Conectify.Services.Library;
using Conectify.Services.SmartThings.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Conectify.Services.SmartThings.Services;

public class SmartThingsService(SmartThingsConfiguration smartThingsConfiguration, IServicesWebsocketClient websocket, IConnectorService connectorService, SmartThingsAuthService smartThingsAuthService, ConectifyDb context)
{
    public async Task RegisterAllDevices(CancellationToken cancellationToken)
    {
        HttpClient httpClient = new();
        var accessToken = await smartThingsAuthService.GetAccessTokenAsync(smartThingsConfiguration.ClientId, smartThingsConfiguration.ClientSecret);

        httpClient.BaseAddress = new Uri("https://api.smartthings.com/v1/");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", accessToken);
        var response = await httpClient.GetAsync("devices");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var devices = JsonSerializer.Deserialize<SmartThingsDeviceResponse>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        foreach (var device in devices?.Items ?? [])
        {
            var conectifyDevice = new ApiDevice()
            {
                Id = device.DeviceId,
                Name = device.Label,
                IPAdress = WebFunctions.GetIPAdress(),
                MacAdress = WebFunctions.GetMacAdress(),
            };

            List<ApiSensor> sensors = [];
            List<ApiActuator> actuators = [];

            string[] capabilities = ["temperatureMeasurement", "relativeHumidityMeasurement", "switch"];
            foreach (string capability in capabilities) {
                var sensor = await GenerateSensor(device, capability, cancellationToken);
                if (sensor != null)
                {
                    sensors.Add(sensor);
                }
            }
            var switchSensor = sensors.FirstOrDefault(x => x.Name == "switch");
            if (switchSensor != null)
            {
                var switchActuator = new ApiActuator()
                {
                    Id = switchSensor.Id,
                    Name = switchSensor.Name,
                    SensorId = switchSensor.Id,
                    SourceDeviceId = device.DeviceId,
                };

                actuators.Add(switchActuator);
            }

            await connectorService.RegisterDevice(conectifyDevice, sensors, actuators, cancellationToken);

        }
    }

    public async Task RefreshAllCapabilities(CancellationToken ct)
    {
        var allCapabilities = await context.SmartThings.ToListAsync(ct);

        foreach (var capability in allCapabilities)
        {
            var result = await RequestCapability(capability, ct);

            if(result != null)
            {
                await websocket.SendMessageAsync(result, ct);
            }
        }
    }

    public async Task<WebsocketEvent?> RequestCapability(SmartThing capability, CancellationToken cancellationToken)
    {
        HttpClient httpClient = new();
        var accessToken = await smartThingsAuthService.GetAccessTokenAsync(smartThingsConfiguration.ClientId, smartThingsConfiguration.ClientSecret);

        httpClient.BaseAddress = new Uri("https://api.smartthings.com/v1/");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer", accessToken);

        var url = $"devices/{capability.DeviceId}/components/main/capabilities/{capability.Capability}/status";
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return capability.Capability switch
        {
            "temperatureMeasurement" => DecodeTemperature(json, capability),
            "relativeHumidityMeasurement" => decodeHumidity(json, capability),
            "switch" => decodeSwitch(json, capability),
            _ => null,
        };
    }

    private WebsocketEvent? decodeSwitch(string json, SmartThing capability)
    {
        var @switch = JsonSerializer.Deserialize<SmartThingsSwitchResponse>(json,
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }) ?? throw new Exception("Switch is null");

        return new WebsocketEvent()
        {
            SourceId = capability.Id,
            Name = "Status",
            Type = Constants.Events.Value,
            NumericValue = @switch.@switch.value == "on" ? 1 :0,
            StringValue = @switch.@switch.value,
            Unit = "",
            TimeCreated = @switch.@switch.timestamp.ToUnixTimeMilliseconds()
        };
    }

    private WebsocketEvent? decodeHumidity(string json, SmartThing capability)
    {
        var temperature = JsonSerializer.Deserialize<SmartThingsHumidityResponse>(json,
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }) ?? throw new Exception("Humidity is null");

        return new WebsocketEvent()
        {
            SourceId = capability.Id,
            Name = "Humidity",
            Type = Constants.Events.Value,
            NumericValue = temperature.humidity.value,
            Unit = temperature.humidity.unit,
            TimeCreated = temperature.humidity.timestamp.ToUnixTimeMilliseconds()
        };
    }

    private WebsocketEvent? DecodeTemperature(string json, SmartThing capability)
    {

        var temperature = JsonSerializer.Deserialize<SmartThingsTemperatureResponse>(json,
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }) ?? throw new Exception("Temperature is null");

        return new WebsocketEvent()
        {
            SourceId = capability.Id,
            Name = "Temperature",
            Type = Constants.Events.Value,
            NumericValue = temperature.temperature.value,
            Unit = temperature.temperature.unit,
            TimeCreated = temperature.temperature.timestamp.ToUnixTimeMilliseconds()
        };
    }

    private async Task<ApiSensor?> GenerateSensor(SmartThingsDevice device, string capability, CancellationToken cancellationToken)
    {
        if (device.Components.SelectMany(x => x.Capabilities.Select(x => x.Id)).Contains(capability))
        {

            var existingCapability = await GetCapability(capability, device.DeviceId, cancellationToken);

            var sensor = new ApiSensor()
            {
                Id = existingCapability ?? Guid.NewGuid(),
                Name = capability,
                SourceDeviceId = device.DeviceId,
            };

            await SaveCapability(sensor, cancellationToken);

            return sensor;
        }

        return null;
    }

    private async Task SaveCapability(ApiSensor sensor, CancellationToken cancellationToken)
    {
        var capability = await context.SmartThings.FirstOrDefaultAsync(x => x.Id == sensor.Id, cancellationToken: cancellationToken);
        if (capability == null)
        {
            await context.SmartThings.AddAsync(new Database.Models.SmartThings.SmartThing { Id = sensor.Id, Capability = sensor.Name, DeviceId = sensor.SourceDeviceId }, cancellationToken);
        }
        else
        {
            capability.Capability = sensor.Name;
            capability.DeviceId = sensor.SourceDeviceId;
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid?> GetCapability(string capability, Guid deviceId, CancellationToken ct)
    {
        var result = await context.SmartThings.AsNoTracking().FirstOrDefaultAsync(x => x.Capability == capability && x.DeviceId == deviceId, cancellationToken: ct);

        return result?.Id ?? null;
    }
}
