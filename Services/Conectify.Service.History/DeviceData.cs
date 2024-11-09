namespace Conectify.Service.History;

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
        Name = "History service"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
        {
            new()
            {
                Id = configuration.SensorId,
                Name = "History Service Sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>()
        {
            new()
            {
                Id = configuration.ActuatorId,
                Name = "History Service Actuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        };

    public IEnumerable<ApiPreference> Preferences =>
    [
                new()
        {
            EventType = Constants.Events.Value
        },
                        new()
        {
            EventType = Constants.Events.ActionResponse
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

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>()
    {
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
    };
}
