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
    private const float OnThreshold = 10f;   // idle -> drawing power
    private const float OffThreshold = 1f;    // drawing power -> idle

    public DateTime LastSent { get; set; } = DateTime.MinValue;

    public float LastSentValue { get; set; }

    public List<float> Values { get; set; } = [];

    /// <summary>
    /// Averages incoming power and only returns a value to forward once per <paramref name="interval"/>.
    /// Two transitions bypass the throttle and forward the new reading immediately:
    ///  - the running average is below 10W and the new reading is above 10W (device turned on),
    ///  - the running average is above 1W and the new reading is below 1W (device turned off).
    /// </summary>
    public float? ProcessValue(float? value, TimeSpan interval)
    {
        if (value is null) return null;

        var newValue = value.Value;

        var average = Values.Count > 0 ? Values.Sum() / Values.Count : LastSentValue;

        var turnedOn = average < OnThreshold && newValue > OnThreshold;
        var turnedOff = average > OffThreshold && newValue < OffThreshold;

        if (turnedOn || turnedOff)
        {
            return Flush(newValue);
        }

        Values.Add(newValue);

        if (DateTime.UtcNow - LastSent > interval)
        {
            return Flush(Values.Sum() / Values.Count);
        }

        return null;
    }

    private float Flush(float valueToSend)
    {
        Values.Clear();
        LastSent = DateTime.UtcNow;
        LastSentValue = valueToSend;
        return valueToSend;
    }
}