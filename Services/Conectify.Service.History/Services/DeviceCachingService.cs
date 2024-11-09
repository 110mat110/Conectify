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
    private IDictionary<Guid, DateTime> actuatorCache = new Dictionary<Guid, DateTime>();
	private readonly IConnectorService connectorService;

	public DeviceCachingService(IConnectorService connectorService)
	{
		this.connectorService = connectorService;
		ReloadActuators();
	}

    public void ObserveSensorFromEvent(Event value)
    {
        if(value.Type == Constants.Events.Value || value.Type == Constants.Events.Action)
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
    }

    public IEnumerable<Guid> GetActiveSensors()
	{
		ReloadSensors();
		return sensorsCache.Keys;
	}

	public IEnumerable<Guid> GetActiveActuators()
    {
        ReloadActuators();
        return actuatorCache.Keys;
    }

	public void Reset()
	{
		ReloadSensors();
		ReloadActuators();
	}

    private void ReloadActuators()
    {
        actuatorCache.Clear();

        foreach (var result in connectorService.LoadAllActuators().Result)
        {
            actuatorCache.TryAdd(result.Id, DateTime.UtcNow);
        }
    }

    private void ReloadSensors()
	{
		var yesterday = DateTime.UtcNow.AddDays(-1);
		sensorsCache = sensorsCache.Where(sensor => sensor.Value.CompareTo(yesterday) >= 0).ToDictionary(x => x.Key, x => x.Value);
	}
}
