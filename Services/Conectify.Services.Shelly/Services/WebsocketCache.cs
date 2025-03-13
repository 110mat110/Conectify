using Conectify.Services.Shelly.Models.Shelly;
using System.Net.WebSockets;

namespace Conectify.Services.Shelly.Services;

public class WebsocketCache
{
    public Dictionary<string, ShellyDeviceCacheItem> Cache = [];
    public Dictionary<Guid, ShellyFequentValueCahceItem> FrequentValueCahce = [];
}

public class ShellyDeviceCacheItem
{
    public string ShellyId { get; set; }
    public WebSocket WebSocket { get; set; }
    public IShelly Shelly { get; set; }
}

public class ShellyFequentValueCahceItem
{
    public DateTime LastSent { get; set; } = DateTime.MinValue;

    public List<float> Values { get; set; } = [];

    public float? ProcessValue(float? value, TimeSpan interval)
    {
        if (value == null) return null;

        Values.Add(value.Value);
        if (DateTime.UtcNow - LastSent > interval)
        {
            var result = Values.Sum() / Values.Count;

            Values.Clear();
            LastSent = DateTime.UtcNow;
            return result;
        }
        return null;
    }
}