using Conectify.Database.Models.Values;
using Conectify.Services.Library;

namespace Conectify.Service.History.Services;

public interface IDeviceCachingService
{
    IEnumerable<Guid> GetActiveActuators();
    IEnumerable<Guid> GetActiveSensors();
    void ObserveActuatorFromResponse(ActionResponse actionResponse);
    void ObserveSensorFromValue(Value value);

    void ObserveSensorFromAction(Database.Models.Values.Action action);

    void Reset();
}

public class DeviceCachingService : IDeviceCachingService
{
    private IDictionary<Guid, DateTime> sensorsCache = new Dictionary<Guid, DateTime>();
    private readonly IDictionary<Guid, DateTime> actuatorCache = new Dictionary<Guid, DateTime>();
	private readonly IConnectorService connectorService;

	public DeviceCachingService(IConnectorService connectorService)
	{
		this.connectorService = connectorService;
		ReloadActuators();
	}

	public void ObserveSensorFromValue(Value value)
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

    public void ObserveActuatorFromResponse(ActionResponse actionResponse)
    {
        if (actuatorCache.ContainsKey(actionResponse.SourceId))
        {
            actuatorCache[actionResponse.SourceId] = DateTime.UtcNow;
        }
        else
        {
            actuatorCache.Add(actionResponse.SourceId, DateTime.UtcNow);
        }
    }

    public IEnumerable<Guid> GetActiveSensors()
	{
		ReloadSensors();
		return sensorsCache.Keys;
	}

	public IEnumerable<Guid> GetActiveActuators()
    {
        return actuatorCache.Keys;
    }

    public void ObserveSensorFromAction(Database.Models.Values.Action action)
    {
        if (sensorsCache.ContainsKey(action.SourceId))
        {
            sensorsCache[action.SourceId] = DateTime.UtcNow;
        }
        else
        {
            sensorsCache.Add(action.SourceId, DateTime.UtcNow);
        }
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
