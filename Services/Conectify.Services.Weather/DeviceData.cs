using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Services;

namespace Conectify.Services.Weather;

public class DeviceData(Configuration configuration) : IDeviceData
{
    public ApiDevice Device => new()
    {
        Id = configuration.DeviceId,
        IPAdress = configuration.ExternalAddress,
        MacAdress = WebFunctions.GetMacAdress(),
        Name = "Ostrava weather"
    };

    public IEnumerable<ApiSensor> Sensors =>
    [
        new()
        {
            Id = configuration.TemperatureSensorId,
            Name = "Temperature",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.WindSpeedSensorId,
            Name = "Wind speed",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.WindDirectionSensorId,
            Name = "Wind direction",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.SunIntensitySensorId,
            Name = "Sun intensity",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.SunAzimuthSensorId,
            Name = "Sun azimuth",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.SunElevationSensorId,
            Name = "Sun elevation",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.RainSensorId,
            Name = "Rain",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.RainForecastSensorId,
            Name = "Rain forecast 24h",
            SourceDeviceId = configuration.DeviceId,
        },
        new()
        {
            Id = configuration.WarningSensorId,
            Name = "Weather warnings",
            SourceDeviceId = configuration.DeviceId,
        }
    ];

    public IEnumerable<ApiPreference> Preferences => [];

    public IEnumerable<MetadataServiceConnector> MetadataConnectors => [];

    public IEnumerable<ApiActuator> Actuators => [];
}
