namespace Conectify.Service.History;

public class Configuration(IConfiguration configuration) : Conectify.Services.Library.ConfigurationBase(configuration)
{
    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }
}
