namespace Conectify.Service.History;

using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

public class DeviceData : IDeviceData
{
    private readonly Configuration configuration;

    public DeviceData(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "History service"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
        {
            new ApiSensor()
            {
                Id = configuration.SensorId,
                Name = "History Service Sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>()
        {
            new ApiActuator()
            {
                Id = configuration.ActuatorId,
                Name = "History Service Actuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        };

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>()
    {
        new ApiPreference()
        {
            SubToValues = true,
            SubToActionResponse = true,
            SubToActions = true,
            SubToCommandResponse = true,
        }
    };

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>()
    {
        new MetadataServiceConnector()
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
