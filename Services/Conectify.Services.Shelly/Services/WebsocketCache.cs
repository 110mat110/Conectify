using Conectify.Services.Shelly.Models.Shelly;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;
using WsSwitch = Conectify.Services.Shelly.Models.Switch;

namespace Conectify.Services.Shelly.Services;

public class WebsocketCache(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Written by the websocket handlers, read by the dashboard page - hence concurrent.
    /// </summary>
    public ConcurrentDictionary<string, ShellyDeviceCacheItem> Cache = new();

    /// <summary>
    /// Status a device announced before it had identified itself, replayed once it has. Without it
    /// the state in that first message is lost and the device stays silent until someone changes it.
    /// </summary>
    public ConcurrentDictionary<string, Models.ShellyWS> PendingStatus = new();

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

    /// <summary>Model code from Shelly.GetDeviceInfo, e.g. "S3SW-002P16EU".</summary>
    public string? Model { get; set; }
    public string? Firmware { get; set; }
    public string? Mac { get; set; }

    /// <summary>
    /// Where the device's own web UI lives. The device reports it as wifi.sta_ip; until it does,
    /// we fall back to the address its websocket connected from.
    /// </summary>
    public string? IpAddress { get; set; }

    public string? Ssid { get; set; }
    public int? Rssi { get; set; }
    public bool? CloudConnected { get; set; }
    public TimeSpan? Uptime { get; set; }
    public double? TemperatureC { get; set; }

    public DateTimeOffset FirstSeen { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.Now;

    /// <summary>Latest relay state per switch channel.</summary>
    public ConcurrentDictionary<int, bool> SwitchOutputs { get; } = new();

    /// <summary>Latest power draw (W) per switch channel.</summary>
    public ConcurrentDictionary<int, float> SwitchPower { get; } = new();

    /// <summary>Latest power draw (W) per standalone meter channel (pm1 / em).</summary>
    public ConcurrentDictionary<int, float> MeterPower { get; } = new();

    public bool IsOnline => WebSocket?.State == WebSocketState.Open;

    public string DisplayName => string.IsNullOrWhiteSpace(Shelly?.Name) ? ShellyId : Shelly.Name;

    public string? WebUrl => string.IsNullOrWhiteSpace(IpAddress) ? null : $"http://{IpAddress}";

    /// <summary>
    /// Folds whatever the device just reported into the state the dashboard renders. Everything is
    /// optional - Shelly sends full status on connect and sparse deltas afterwards, so a field is
    /// only overwritten when the message actually carries it.
    /// </summary>
    public void Record(Models.ShellyWS message, WebSocket source, string? remoteIp)
    {
        // A reconnecting device keeps its cache entry but arrives on a new socket.
        WebSocket = source;
        LastSeen = DateTimeOffset.Now;

        if (string.IsNullOrWhiteSpace(IpAddress) && !string.IsNullOrWhiteSpace(remoteIp))
        {
            IpAddress = remoteIp;
        }

        if (message.Result is not null)
        {
            Model = message.Result.Model ?? Model;
            Firmware = message.Result.Ver ?? Firmware;
            Mac = message.Result.Mac ?? Mac;

            // Wifi.GetStatus reply.
            if (!string.IsNullOrWhiteSpace(message.Result.StaIp))
            {
                IpAddress = message.Result.StaIp;
            }

            Ssid = message.Result.Ssid ?? Ssid;
            Rssi = message.Result.Rssi ?? Rssi;
        }

        var parameters = message.Params;
        if (parameters is null)
        {
            return;
        }

        if (parameters.Wifi is not null)
        {
            // The device knows its own LAN address - trust it over the socket's remote address.
            if (!string.IsNullOrWhiteSpace(parameters.Wifi.StaIp))
            {
                IpAddress = parameters.Wifi.StaIp;
            }

            Ssid = parameters.Wifi.Ssid ?? Ssid;
            Rssi = parameters.Wifi.Rssi;
        }

        if (parameters.Cloud is not null)
        {
            CloudConnected = parameters.Cloud.Connected;
        }

        if (parameters.Sys is not null)
        {
            Mac = parameters.Sys.Mac ?? Mac;
            if (parameters.Sys.Uptime > 0)
            {
                Uptime = TimeSpan.FromSeconds(parameters.Sys.Uptime);
            }
        }

        RecordSwitch(0, parameters.Switch0);
        RecordSwitch(1, parameters.Switch1);
        RecordSwitch(2, parameters.Switch2);
        RecordSwitch(3, parameters.Switch3);

        if (parameters.Pm0?.apower is not null)
        {
            MeterPower[parameters.Pm0.Id] = parameters.Pm0.apower.Value;
        }

        if (parameters.em0 is not null)
        {
            MeterPower[0] = Math.Abs(parameters.em0.total_act_power);
        }
    }

    private void RecordSwitch(int channel, WsSwitch? sw)
    {
        if (sw is null)
        {
            return;
        }

        // Older firmware reports the relay as "on", newer as "output".
        SwitchOutputs[channel] = sw.Output ?? sw.on;

        if (sw.Temperature is not null)
        {
            TemperatureC = sw.Temperature.TC;
        }

        // by_minute[0] is mWh accumulated over the last minute; x60/1000 turns it into watts.
        if (sw.aenergy?.ByMinute is { Length: > 0 } byMinute)
        {
            SwitchPower[channel] = byMinute[0] * 60 / 1000;
        }
    }
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
