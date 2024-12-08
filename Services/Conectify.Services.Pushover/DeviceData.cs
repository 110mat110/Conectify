using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.Pushover;

public class DeviceData(Configuration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Pushover"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
    {
        new()
        {
            Id = configuration.SensorId,
            Name = "Pushover",
            SourceDeviceId = configuration.DeviceId,
        }
    };

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>();

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>();

    public IEnumerable<ApiActuator> Actuators =>
    [
        new()
        {
            Id = configuration.SensorId,
            Name = "Pushover",
            SourceDeviceId = configuration.DeviceId,
            SensorId = configuration.SensorId
        }
    ];
}
