namespace Conectify.Services.MQTTTasker;

using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

public class DeviceData(Configuration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "MQTT"
    };

    public IEnumerable<ApiSensor> Sensors => GenerateSensors();

    public IEnumerable<ApiActuator> Actuators =>
        [
            new()
            {
                Id = configuration.ActuatorId,
                Name = "MQTTActuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        ];

    public IEnumerable<ApiPreference> Preferences =>
    [
        new()
        {
            EventType = Constants.Events.Value
        }
    ];

    public IEnumerable<MetadataServiceConnector> MetadataConnectors =>
    [
        new()
        {
            MaxVal = 1,
            MinVal = 0,
            MetadataName = "Visible",
            NumericValue = 0,
            StringValue = string.Empty,
            TypeValue = 0,
            Unit = string.Empty,
        }
    ];

    private List<ApiSensor> GenerateSensors()
    {
        var sensors = new List<ApiSensor>()
        {
            new()
            {
                Id = configuration.SensorId,
                Name = configuration.DeviceName,
                SourceDeviceId = configuration.DeviceId,
            }
        };
        return sensors;
    }
}
