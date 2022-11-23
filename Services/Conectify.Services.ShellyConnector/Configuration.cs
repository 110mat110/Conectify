namespace Conectify.Services.ShellyConnector;

public class Configuration : Conectify.Services.Library.Configuration
{
    public Configuration(IConfiguration configuration) : base(configuration)
    {
    }
    public Guid SensorId { get; set; }

    public Guid ActuatorId { get; set; }

    public string ShellyIp { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    public Guid LongPressSensorId { get; set; } = Guid.Empty;

    public int LongPressDefaultValue { get; set; } = 0;

    public string ShellyType { get; set; } = string.Empty;

    public Guid PowerSensorId { get; set; } = Guid.Empty;
}
