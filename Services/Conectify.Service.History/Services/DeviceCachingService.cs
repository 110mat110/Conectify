using Conectify.Database.Models.Values;
using Conectify.Services.Library;
using Conectify.Shared.Library;

namespace Conectify.Service.History.Services;

public interface IDeviceCachingService
{
    IEnumerable<Guid> GetActiveActuators();

    IEnumerable<Guid> GetActiveSensors();

    void ObserveSensorFromEvent(Event value);

    void Reset();
}

public class DeviceCachingService : IDeviceCachingService
{
    private IDictionary<Guid, DateTime> sensorsCache = new Dictionary<Guid, DateTime>();
    private readonly IDictionary<Guid, DateTime> actuatorCache = new Dictionary<Guid, DateTime>();
    private readonly IConnectorService connectorService;
    private readonly ILogger<DeviceCachingService> logger;

    public DeviceCachingService(IConnectorService connectorService, ILogger<DeviceCachingService> logger)
    {
        this.connectorService = connectorService;
        this.logger = logger;
        ReloadActuators(Guid.NewGuid());
    }

    public void ObserveSensorFromEvent(Event value)
    {
        Tracing.Trace(() =>
        {
            if (value.Type == Constants.Events.Value || value.Type == Constants.Events.Action)
            {
                if (sensorsCache.ContainsKey(value.SourceId))
                {
                    sensorsCache[value.SourceId] = DateTime.UtcNow;
                }
                else
                {
                    sensorsCache.Add(value.SourceId, DateTime.UtcNow);
                }
            }
        }, value.Id, "Observing sensor from event");
    }

    public IEnumerable<Guid> GetActiveSensors()
    {
        ReloadSensors(Guid.NewGuid());
        return sensorsCache.Keys;
    }

    public IEnumerable<Guid> GetActiveActuators()
    {
        ReloadActuators(Guid.NewGuid());
        return actuatorCache.Keys;
    }

    public void Reset()
    {
        var tracingId = Guid.NewGuid();
        Tracing.Trace(() =>
        {
            ReloadSensors(tracingId);
            ReloadActuators(tracingId);
        }, tracingId, "Reset device cache");

    }

    private void ReloadActuators(Guid traceId)
    {
        Tracing.Trace(() =>
        {
            actuatorCache.Clear();

            foreach (var result in connectorService.LoadAllActuators().Result)
            {
                actuatorCache.TryAdd(result.Id, DateTime.UtcNow);
            }

            logger.LogInformation("ReloadActuators: loaded {Count} actuator(s) into cache", actuatorCache.Count);
        }, traceId, "Reload Actuators");

    }

    private void ReloadSensors(Guid traceId)
    {
        Tracing.Trace(() =>
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var before = sensorsCache.Count;
            sensorsCache = sensorsCache.Where(sensor => sensor.Value.CompareTo(yesterday) >= 0).ToDictionary(x => x.Key, x => x.Value);
            logger.LogInformation("ReloadSensors: {After} active sensor(s) ({Evicted} evicted)", sensorsCache.Count, before - sensorsCache.Count);
        }, traceId, "Reload Sensors");
    }
}
