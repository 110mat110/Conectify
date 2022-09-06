namespace Conectify.Services.ShellyConnector;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models;

public class DeviceData : IDeviceData
{
    private readonly Configuration configuration;

    public DeviceData(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public ApiDevice ApiDevice => new ApiDevice()
    {
        Id = configuration.DeviceId,
        IPAdress = "192.168.1.1",
        MacAdress = "xx.xx.xx",
        Name = "Shelly server"
    };

    public IEnumerable<ApiSensor> ApiSensors => new List<ApiSensor>()
        {
            new ApiSensor()
            {
                Id = configuration.SensorId,
                Name = "TestSensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

    public IEnumerable<ApiActuator> ApiActuators => new List<ApiActuator>()
        {
            new ApiActuator()
            {
                Id = configuration.ActuatorId,
                Name = "TestActuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        };
}
