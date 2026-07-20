namespace Conectify.Services.Weather;

public class Configuration(IConfiguration configuration) : Library.ConfigurationBase(configuration)
{
    public double Latitude { get; set; } = 0;
    public double Longitude { get; set; } = 0;
    public string WarningRegion { get; set; } = "";
    public int RefreshIntervalMinutes { get; set; } = 10;

    public Guid TemperatureSensorId { get; set; }
    public Guid WindSpeedSensorId { get; set; }
    public Guid WindDirectionSensorId { get; set; }
    public Guid SunIntensitySensorId { get; set; }
    public Guid SunAzimuthSensorId { get; set; }
    public Guid SunElevationSensorId { get; set; }
    public Guid RainSensorId { get; set; }
    public Guid RainForecastSensorId { get; set; }
    public Guid WarningSensorId { get; set; }
}
