using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.AQIScraper;

public class DeviceData(Configuration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Poruba weather"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
    {
        new ApiSensor()
        {
            Id = configuration.SensorId,
            Name = "AQI",
            SourceDeviceId = configuration.DeviceId,
        }
    };

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>();

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>();

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>();
}
