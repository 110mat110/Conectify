using Conectify.Services.Library;

namespace Conectify.Services.Cloud;

public class CloudConfiguration(IConfiguration configuration) : ConfigurationBase(configuration)
{
    public string BaseAddress { get; set; } = string.Empty;

    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }
}
