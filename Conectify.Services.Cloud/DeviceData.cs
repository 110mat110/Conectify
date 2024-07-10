using Conectify.Services.Cloud;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.Cloud;

public class DeviceData(CloudConfiguration configuration) : IDeviceData
{
    private readonly CloudConfiguration configuration = configuration;

    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = WebFunctions.GetIPAdress(),
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Cloud service"
    };

    public IEnumerable<ApiSensor> Sensors => new List<ApiSensor>()
        {
            new ApiSensor()
            {
                Id = configuration.SensorId,
                Name = "Cloud sensor",
                SourceDeviceId = configuration.DeviceId,
            }
        };

    public IEnumerable<ApiActuator> Actuators => new List<ApiActuator>()
        {
            new ApiActuator()
            {
                Id = configuration.ActuatorId,
                Name = "Cloud actuator",
                SourceDeviceId = configuration.DeviceId,
                SensorId = configuration.SensorId
            }
        };

    public IEnumerable<ApiPreference> Preferences => new List<ApiPreference>()
    {
        new ApiPreference()
        {
            SubToValues = true,
            SubToActions = true,
            SubToCommands = true,
        }
    };

	public IEnumerable<MetadataServiceConnector> MetadataConnectors => new List<MetadataServiceConnector>()
	{
		new MetadataServiceConnector()
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
