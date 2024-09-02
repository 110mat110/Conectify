namespace Conectify.Services.ShellyConnector;

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

    public IEnumerable<ApiSensor> Sensors => GenerateSensors();

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>()
        {
            new ApiActuator()
            {
                Id = configuration.ActuatorId,
                Name =  configuration.DeviceName,
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        };

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>();

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>();

    private IEnumerable<ApiSensor> GenerateSensors()
    {
        var sensors = new List<ApiSensor>()
        {
            new ApiSensor()
            {
                Id = configuration.SensorId,
                Name = configuration.DeviceName,
                SourceDeviceId = configuration.DeviceId,
            }
        };

        if (configuration.LongPressSensorId != Guid.Empty)
        {
            sensors.Add(new ApiSensor()
            {
                Id = configuration.LongPressSensorId,
                Name = configuration.DeviceName + " - Long Press",
                SourceDeviceId = configuration.DeviceId,
            });
        }

        if (configuration.PowerSensorId != Guid.Empty)
        {
            sensors.Add(new ApiSensor()
            {
                Id = configuration.PowerSensorId,
                Name = configuration.DeviceName + " - Power",
                SourceDeviceId = configuration.DeviceId,
            });
        }
        return sensors;
    }
}
