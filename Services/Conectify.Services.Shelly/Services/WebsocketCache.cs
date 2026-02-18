using Conectify.Services.Shelly.Models.Shelly;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;

namespace Conectify.Services.Shelly.Services;

public class WebsocketCache(IServiceProvider serviceProvider)
{
    public Dictionary<string, ShellyDeviceCacheItem> Cache = [];
    public Dictionary<Guid, ShellyFequentValueCahceItem> FrequentValueCahce = [];

    public void ProcessInboundDuration(TimeSpan duration)
    {
        var meterFactory = serviceProvider.GetService<IMeterFactory>();
        if (meterFactory is null) return;

            var meter = meterFactory.Create("CustomMeters");
            if (meter is null) return;
            var inboundHistogram = meter.CreateHistogram<double>("Shelly_inbound_reading_time_ms", "ms");
            if(inboundHistogram is null) return;
            // Use pre-created histogram instrument
            inboundHistogram?.Record(duration.TotalMilliseconds);
    }

    public List<Tuple<DateTime, TimeSpan>> OutboundDurations = [];

    public void ProcessOutbandDuration(TimeSpan duration)
    {
        var meterFactory = serviceProvider.GetService<IMeterFactory>();
        if (meterFactory is null) return;

        var meter = meterFactory.Create("CustomMeters");
        if (meter is null) return;
        var outboundHistogram = meter.CreateHistogram<double>("Shelly_outband_reading_time_ms", "ms");
        if (outboundHistogram is null) return;
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