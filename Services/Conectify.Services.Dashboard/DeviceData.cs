namespace Conectify.Services.Dashboard;

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
        Name = "Dashboard"
    };

    public IEnumerable<ApiSensor> Sensors => GenerateSensors();

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>();

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>();

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>();

    private IEnumerable<ApiSensor> GenerateSensors()
    {
        var sensors = new List<ApiSensor>()
        {
            new ApiSensor()
            {
                Id = configuration.SensorId,
                Name = "Dashboard Sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

        return sensors;
    }
}
