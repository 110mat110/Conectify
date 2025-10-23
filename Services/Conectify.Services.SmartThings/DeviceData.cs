namespace Conectify.Services.SmartThings;

using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

public class DeviceData(SmartThingsConfiguration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "SmartThings"
    };

    public IEnumerable<ApiSensor> Sensors => [];
    public IEnumerable<ApiActuator> Actuators => [];

    public IEnumerable<ApiPreference> Preferences => [];

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => [];
}
