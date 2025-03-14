namespace Conectify.Services.Pushover;

public class Configuration(IConfiguration configuration) : Library.ConfigurationBase(configuration)
{
    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }

    public string Token { get; set; }
    public string ClientKey { get; set; }
}