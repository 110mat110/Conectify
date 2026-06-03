using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.Android;

public class DeviceData(AndroidConfiguration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Android Widget Service"
    };

    public IEnumerable<ApiSensor> Sensors =>
        [
            new()
            {
                Id = configuration.SensorId,
                Name = "Automatization 2 Sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        ];

    public IEnumerable<ApiActuator> Actuators =>
        [
            new()
            {
                Id = configuration.ActuatorId,
                Name = "Automatization 2 Actuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        ];

    public IEnumerable<ApiPreference> Preferences =>
    [
        new()
        {
            EventType = Constants.Events.All
        }
    ];

    public IEnumerable<MetadataServiceConnector> MetadataConnectors =>
    [
        new()
        {
            MaxVal = 1,
            MinVal = 0,
            MetadataName = Constants.Metadatas.Visible,
            NumericValue = 0,
            StringValue = string.Empty,
            TypeValue = 0,
            Unit = string.Empty,
        }
    ];
}
