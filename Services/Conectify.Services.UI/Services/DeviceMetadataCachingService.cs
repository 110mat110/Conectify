using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.UI.Services;

public interface IDeviceMetadataCachingService
{
    List<ApiMetadata> GetSensorMetadata(Guid sensorId);
    List<ApiMetadata> GetActuatorMetadata(Guid actuatorId);
    string GetSensorName(Guid sensorId);
    string GetActuatorName(Guid actuatorId);
    Guid GetActuatorSensorId(Guid actuatorId);
    string GetDeviceName(Guid deviceId);
    Guid GetSensorDeviceId(Guid sensorId);
    Guid GetActuatorDeviceId(Guid actuatorId);
}

public class DeviceMetadataCachingService : IDeviceMetadataCachingService, IDisposable
{
    private record CachedSensor(string Name, Guid SourceDeviceId, List<ApiMetadata> Metadata);
    private record CachedActuator(string Name, Guid SourceDeviceId, Guid SensorId, List<ApiMetadata> Metadata);

    private Dictionary<Guid, CachedSensor> _sensors = [];
    private Dictionary<Guid, CachedActuator> _actuators = [];
    private Dictionary<Guid, string> _deviceNames = [];
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceMetadataCachingService> _logger;
    private readonly Timer _refreshTimer;
    private readonly object _lock = new();

    public DeviceMetadataCachingService(IServiceProvider serviceProvider, ILogger<DeviceMetadataCachingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        Reload();
        _refreshTimer = new Timer(_ => Reload(), null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
    }

    private void Reload()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

            var devices = db.Devices
                .AsNoTracking()
                .Include(d => d.Metadata).ThenInclude(m => m.Metadata)
                .ToDictionary(
                    d => d.Id,
                    d => new
                    {
                        d.Name,
                        Metadata = d.Metadata.Select(MapMetadata).ToList()
                    });

            var sensors = db.Sensors
                .AsNoTracking()
                .Include(s => s.Metadata).ThenInclude(m => m.Metadata)
                .ToDictionary(s => s.Id, s =>
                {
                    var sensorMeta = s.Metadata.Select(MapMetadata).ToList();
                    if (devices.TryGetValue(s.SourceDeviceId, out var device))
                    {
                        var deviceOnly = device.Metadata.Where(dm => !sensorMeta.Any(sm => sm.Name == dm.Name));
                        sensorMeta.AddRange(deviceOnly);
                    }
                    return new CachedSensor(s.Name, s.SourceDeviceId, sensorMeta);
                });

            var actuators = db.Actuators
                .AsNoTracking()
                .Include(a => a.Metadata).ThenInclude(m => m.Metadata)
                .ToDictionary(a => a.Id, a =>
                {
                    var actuatorMeta = a.Metadata.Select(MapMetadata).ToList();
                    if (devices.TryGetValue(a.SourceDeviceId, out var device))
                    {
                        var deviceOnly = device.Metadata.Where(dm => !actuatorMeta.Any(am => am.Name == dm.Name));
                        actuatorMeta.AddRange(deviceOnly);
                    }
                    return new CachedActuator(a.Name, a.SourceDeviceId, a.SensorId, actuatorMeta);
                });

            lock (_lock)
            {
                _sensors = sensors;
                _actuators = actuators;
                _deviceNames = devices.ToDictionary(d => d.Key, d => d.Value.Name);
            }

            _logger.LogInformation("Cache reloaded: {SensorCount} sensors, {ActuatorCount} actuators, {DeviceCount} devices",
                sensors.Count, actuators.Count, devices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload device metadata cache");
        }
    }

    private static ApiMetadata MapMetadata<T>(MetadataConnector<T> mc) where T : class => new()
    {
        Name = mc.Metadata.Name,
        NumericValue = mc.NumericValue,
        StringValue = mc.StringValue,
        Unit = mc.Unit,
        MinVal = mc.MinVal,
        MaxVal = mc.MaxVal,
        MetadataId = mc.MetadataId,
        Id = mc.Id,
    };

    public List<ApiMetadata> GetSensorMetadata(Guid sensorId)
    {
        lock (_lock) { return _sensors.TryGetValue(sensorId, out var s) ? s.Metadata : []; }
    }

    public List<ApiMetadata> GetActuatorMetadata(Guid actuatorId)
    {
        lock (_lock) { return _actuators.TryGetValue(actuatorId, out var a) ? a.Metadata : []; }
    }

    public string GetSensorName(Guid sensorId)
    {
        lock (_lock) { return _sensors.TryGetValue(sensorId, out var s) ? s.Name : sensorId.ToString(); }
    }

    public string GetActuatorName(Guid actuatorId)
    {
        lock (_lock) { return _actuators.TryGetValue(actuatorId, out var a) ? a.Name : actuatorId.ToString(); }
    }

    public Guid GetActuatorSensorId(Guid actuatorId)
    {
        lock (_lock) { return _actuators.TryGetValue(actuatorId, out var a) ? a.SensorId : Guid.Empty; }
    }

    public string GetDeviceName(Guid deviceId)
    {
        lock (_lock) { return _deviceNames.TryGetValue(deviceId, out var name) ? name : deviceId.ToString(); }
    }

    public Guid GetSensorDeviceId(Guid sensorId)
    {
        lock (_lock) { return _sensors.TryGetValue(sensorId, out var s) ? s.SourceDeviceId : Guid.Empty; }
    }

    public Guid GetActuatorDeviceId(Guid actuatorId)
    {
        lock (_lock) { return _actuators.TryGetValue(actuatorId, out var a) ? a.SourceDeviceId : Guid.Empty; }
    }

    public void Dispose() => _refreshTimer.Dispose();
}
