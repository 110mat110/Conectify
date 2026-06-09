using Conectify.Services.Library;

namespace Conectify.Services.Cloud.Services;

public class DeviceService(IConnectorService connectorService, ILogger<DeviceService> logger)
{
    private readonly Dictionary<Guid, Guid> devices = [];

    public Guid? GetDeviceById(Guid sourceId)
    {
        devices.TryGetValue(sourceId, out var device);
        return device;
    }

    public void RegisterDevice(Guid deviceId, Guid sourceId)
    {
        var added = devices.TryAdd(sourceId, deviceId);
        logger.LogInformation("RegisterDevice (inline): deviceId={DeviceId} sourceId={SourceId} added={Added}", deviceId, sourceId, added);
    }

    public async Task RegisterDevice(Guid deviceId, CancellationToken cancellationToken)
    {
        logger.LogInformation("RegisterDevice: loading sensors/actuators for deviceId={DeviceId}", deviceId);
        var sensors = await connectorService.LoadSensorsPerDevice(deviceId, cancellationToken);
        var actuators = await connectorService.LoadActuatorsPerDevice(deviceId, cancellationToken);

        foreach (var actuator in actuators)
        {
            devices.TryAdd(actuator.Id, deviceId);
        }

        foreach (var sensor in sensors)
        {
            devices.TryAdd(sensor.Id, deviceId);
        }

        logger.LogInformation("RegisterDevice: mapped {SensorCount} sensor(s) and {ActuatorCount} actuator(s) for deviceId={DeviceId}",
            sensors.Count(), actuators.Count(), deviceId);
    }
}
