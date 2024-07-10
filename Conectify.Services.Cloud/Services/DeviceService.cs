using Conectify.Services.Library;

namespace Conectify.Services.Cloud.Services;

public class DeviceService(IConnectorService connectorService)
{
    private readonly Dictionary<Guid, Guid> devices = [];

    public Guid? GetDeviceById(Guid sourceId)
    {
        devices.TryGetValue(sourceId, out var device);

        return device;
    }

    public void RegisterDevice(Guid deviceId, Guid sourceId)
    {
        devices.TryAdd(sourceId, deviceId);
    }

    public async Task RegisterDevice(Guid deviceId, CancellationToken cancellationToken)
    {
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
    }
}
