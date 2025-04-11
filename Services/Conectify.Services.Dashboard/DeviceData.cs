namespace Conectify.Services.Dashboard;

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
        Name = "Dashboard"
    };

    public IEnumerable<ApiSensor> Sensors => GenerateSensors();

    public IEnumerable<ApiActuator> Actuators => [];

    public IEnumerable<ApiPreference> Preferences => [];

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => [];

    private List<ApiSensor> GenerateSensors()
    {
        var sensors = new List<ApiSensor>()
        {
            new()
            {
                Id = configuration.SensorId,
                Name = "Dashboard Sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

        return sensors;
    }
}
