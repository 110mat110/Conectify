using Conectify.Services.Library;

namespace Conectify.Services.Automatization;

public class AutomatizationConfiguration(IConfiguration configuration) : ConfigurationBase(configuration)
{
    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }
}
