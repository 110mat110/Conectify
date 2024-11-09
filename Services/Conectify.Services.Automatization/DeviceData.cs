using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.Automatization;

public class DeviceData(AutomatizationConfiguration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Automatization service"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
        {
            new()
            {
                Id = configuration.SensorId,
                Name = "Automatization Sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>()
        {
            new()
            {
                Id = configuration.ActuatorId,
                Name = "Automatization Actuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        };

    public IEnumerable<ApiPreference> Preferences =>
    [
        new()
        {
            EventType = Constants.Events.All
        }
    ];

	public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>()
	{
		new()
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
}
