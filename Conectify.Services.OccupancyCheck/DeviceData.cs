using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.OccupancyCheck;

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
        Name = "Occupancy check"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
    {
        new ApiSensor()
        {
            Id = configuration.SensorId,
            Name = "Occupancy",
            SourceDeviceId = configuration.DeviceId,
        }
    };

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>();

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

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>();
}
