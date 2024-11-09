namespace Conectify.Services.Shelly;

public class Configuration(IConfiguration configuration) : Library.ConfigurationBase(configuration)
{
    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }
}
