namespace Conectify.Server.Caches;

public interface IDataCache
{
    void AddLastCall(Guid deviceId);
    long GetLastCall(Guid deviceId);
}

public class DataCache : IDataCache
{
    private Dictionary<Guid, long> deviceCalls = [];

    public void AddLastCall(Guid deviceId)
    {
        if (deviceCalls.ContainsKey(deviceId))
        {
            deviceCalls[deviceId] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        else
        {
            deviceCalls.Add(deviceId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }

    public long GetLastCall(Guid deviceId)
    {
        if (deviceCalls.TryGetValue(deviceId, out long value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }
}
