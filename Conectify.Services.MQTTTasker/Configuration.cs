namespace Conectify.Services.MQTTTasker;

public class Configuration : Library.ConfigurationBase
{
    public Configuration(IConfiguration configuration) : base(configuration)
    {
    }
    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string Broker { get; set; } = string.Empty;
}
