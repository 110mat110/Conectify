using Conectify.Services.Shelly.Models.Shelly;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;

namespace Conectify.Services.Shelly.Services;

public class WebsocketCache
{
    public Dictionary<string, ShellyDeviceCacheItem> Cache = [];
    public Dictionary<Guid, ShellyFequentValueCahceItem> FrequentValueCahce = [];
    public List<Tuple<DateTime,TimeSpan>> InboundDurations = [];

    private readonly IMeterFactory? meterFactory;
    private readonly Meter? meter;
    private readonly Histogram<double>? inboundHistogram;
    private readonly Histogram<double>? outboundHistogram;

    public WebsocketCache(IServiceProvider serviceProvider)
    {
        // Resolve IMeterFactory once and create instruments once.
        meterFactory = serviceProvider.GetService<IMeterFactory>();
        if (meterFactory is not null)
        {
            meter = meterFactory.Create("CustomMeters");
            inboundHistogram = meter.CreateHistogram<double>("Shelly_inbound_reading_time_ms", "ms");
            outboundHistogram = meter.CreateHistogram<double>("Shelly_outband_reading_time_ms", "ms");
        }
    }

    public void ProcessInboundDuration(TimeSpan duration)
    {
        InboundDurations.Add(new Tuple<DateTime, TimeSpan>(DateTime.UtcNow, duration));
        InboundDurations.Where(x => x.Item1 < DateTime.UtcNow.AddMinutes(-1)).ToList().ForEach(x => InboundDurations.Remove(x));

        var average = TimeSpan.FromMilliseconds(InboundDurations.Sum(x => x.Item2.TotalMilliseconds) / InboundDurations.Count);

        // Use pre-created histogram instrument
        inboundHistogram?.Record(duration.TotalMilliseconds);
    }

    public List<Tuple<DateTime, TimeSpan>> OutboundDurations = [];

    public void ProcessOutbandDuration(TimeSpan duration)
    {
        OutboundDurations.Add(new Tuple<DateTime, TimeSpan>(DateTime.UtcNow, duration));
        OutboundDurations.Where(x => x.Item1 < DateTime.UtcNow.AddMinutes(-1)).ToList().ForEach(x => OutboundDurations.Remove(x));

        var average = TimeSpan.FromMilliseconds(OutboundDurations.Sum(x => x.Item2.TotalMilliseconds) / OutboundDurations.Count);

        // Use pre-created histogram instrument
        outboundHistogram?.Record(duration.TotalMilliseconds);
    }

    public float? ProcessFrequentValue(Guid deviceId, float? value, TimeSpan interval)
    {
        if (!FrequentValueCahce.TryGetValue(deviceId, out var cacheItem))
        {
            cacheItem = new ShellyFequentValueCahceItem() { LastSent = DateTime.MinValue };
            FrequentValueCahce[deviceId] = cacheItem;
        }
        return cacheItem.ProcessValue(value, interval);
    }
}

public class ShellyDeviceCacheItem
{
    public required string ShellyId { get; set; }
    public WebSocket? WebSocket { get; set; }
    public required IShelly Shelly { get; set; }
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