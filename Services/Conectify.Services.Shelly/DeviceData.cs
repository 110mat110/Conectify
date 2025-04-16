namespace Conectify.Services.Shelly;

using Conectify.Services.Library;
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
        Name = "Shelly"
    };

    public IEnumerable<ApiSensor> Sensors => [];
    public IEnumerable<ApiActuator> Actuators => [];

    public IEnumerable<ApiPreference> Preferences => [];

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => [];
}
