using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.Cloud;

public class DeviceData(CloudConfiguration configuration) : IDeviceData
{
    private readonly CloudConfiguration configuration = configuration;

    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Cloud service"
    };

    public IEnumerable<ApiSensor> Sensors =>
        [
            new()
            {
                Id = configuration.SensorId,
                Name = "Cloud sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        ];

    public IEnumerable<ApiActuator> Actuators =>
        [
            new()
            {
                Id = configuration.ActuatorId,
                Name = "Cloud actuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        ];

    public IEnumerable<ApiPreference> Preferences =>
    [
                new()
        {
            EventType = Constants.Events.Value
        },
                        new()
        {
            EventType = Constants.Events.Command
        },
                                new()
        {
            EventType = Constants.Events.Action
        },
                                        new()
        {
            EventType = Constants.Events.CommandResponse
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
}
