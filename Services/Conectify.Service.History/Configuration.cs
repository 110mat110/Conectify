namespace Conectify.Service.History;

public class Configuration : Conectify.Services.Library.ConfigurationBase
{
    public Configuration(IConfiguration configuration) : base(configuration)
    {
    }

    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }
}
