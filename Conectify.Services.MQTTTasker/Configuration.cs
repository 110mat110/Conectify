namespace Conectify.Services.MQTTTasker;

public class Configuration(IConfiguration configuration) : Library.ConfigurationBase(configuration)
{
    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string Broker { get; set; } = string.Empty;
}
