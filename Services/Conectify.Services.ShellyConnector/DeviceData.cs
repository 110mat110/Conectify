namespace Conectify.Services.ShellyConnector;

using Conectify.Database.Models;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models;

public class DeviceData : IDeviceData
{
    private readonly Configuration configuration;

    public DeviceData(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public ApiDevice Device => new ApiDevice()
    {
        Id = configuration.DeviceId,
        IPAdress = "192.168.1.1",
        MacAdress = "xx.xx.xx",
        Name = "Shelly server"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
        {
            new ApiSensor()
            {
                Id = configuration.SensorId,
                Name = "TestSensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>()
        {
            new ApiActuator()
            {
                Id = configuration.ActuatorId,
                Name = "TestActuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        };

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>();
}
