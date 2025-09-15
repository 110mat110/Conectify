using Newtonsoft.Json;

namespace Conectify.Services.Shelly.Models;
#pragma warning disable IDE1006 // Naming Styles File generatet

public class ShellyWS
{
    public string? Src { get; set; }
    public string? Dst { get; set; }
    public string? Method { get; set; }
    public Params? Params { get; set; }
    public Result? Result { get; set; }
}

public class Result
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Mac { get; set; }
    public int Slot { get; set; }
    public string? Model { get; set; }
    public int Gen { get; set; }
    public string? FwId { get; set; }
    public string? Ver { get; set; }
    public string? App { get; set; }
    public bool AuthEn { get; set; }
    public string? AuthDomain { get; set; }
}

public class Params
{
    public double Ts { get; set; }
    public Ble? Ble { get; set; }
    public Cloud? Cloud { get; set; }
    [JsonProperty("input:0")]
    public Input? Input0 { get; set; }
    [JsonProperty("input:1")]
    public Input? Input1 { get; set; }
    [JsonProperty("input:2")]
    public Input? Input2 { get; set; }
    [JsonProperty("input:3")]
    public Input? Input3 { get; set; }
    public Mqtt? Mqtt { get; set; }

    [JsonProperty("switch:0")]
    public Switch? Switch0 { get; set; }
    [JsonProperty("switch:1")]
    public Switch? Switch1 { get; set; }
    [JsonProperty("switch:2")]
    public Switch? Switch2 { get; set; }
    [JsonProperty("switch:3")]
    public Switch? Switch3 { get; set; }
    public Sys? Sys { get; set; }
    public Wifi? Wifi { get; set; }
    public Ws? Ws { get; set; }

    [JsonProperty("pm1:0")]
    public Pm10? Pm0 { get; set; }

    [JsonProperty("em:0")]
    public Em0? em0 { get; set; }
    public Event[]? events { get; set; }
}

public class Pm10
{
    public int Id { get; set; }
    public Energy? aenergy { get; set; }

    public float? apower { get; set; }
}

public class Em0
{
    public float a_act_power { get; set; }
    public float a_aprt_power { get; set; }
    public float a_current { get; set; }
    public float a_freq { get; set; }
    public float a_pf { get; set; }
    public float a_voltage { get; set; }

    public float b_act_power { get; set; }
    public float b_aprt_power { get; set; }
    public float b_current { get; set; }
    public float b_freq { get; set; }
    public float b_pf { get; set; }
    public float b_voltage { get; set; }

    public float c_act_power { get; set; }
    public float c_aprt_power { get; set; }
    public float c_current { get; set; }
    public float c_freq { get; set; }
    public float c_pf { get; set; }
    public float c_voltage { get; set; }

    public float? n_current { get; set; } // nullable because JSON has null

    public float total_act_power { get; set; }
    public float total_aprt_power { get; set; }
    public float total_current { get; set; }
}


public class Event
{
    public string? component { get; set; }

    public int id { get; set; }

    public string? @event { get; set; }
}
public class Ble
{
}

public class Cloud
{
    public bool Connected { get; set; }
}

public class Input
{
    public int Id { get; set; }
    public bool? State { get; set; }
}

public class Mqtt
{
    public bool Connected { get; set; }
}

public class Switch
{
    public int id { get; set; }
    public string? Source { get; set; }
    public bool? Output { get; set; }
    public bool on { get; set; }
    public Temperature? Temperature { get; set; }

    public Energy? aenergy { get; set; }
}

public class Energy
{
    [JsonProperty("by_minute")]
    public float[]? ByMinute { get; set; }

    [JsonProperty("minute_ts")]
    public long MinuteTs { get; set; }
    public double total { get; set; }
}

public class Temperature
{
    public double TC { get; set; }
    public double TF { get; set; }
}

public class Sys
{
    public string? Mac { get; set; }
    public bool RestartRequired { get; set; }
    public object? Time { get; set; }
    public object? Unixtime { get; set; }
    public int Uptime { get; set; }
    public int RamSize { get; set; }
    public int RamFree { get; set; }
    public int FsSize { get; set; }
    public int FsFree { get; set; }
    public int CfgRev { get; set; }
    public int KvsRev { get; set; }
    public int ScheduleRev { get; set; }
    public int WebhookRev { get; set; }
    public AvailableUpdates? AvailableUpdates { get; set; }
    public int ResetReason { get; set; }
}

public class AvailableUpdates
{
}

public class Wifi
{
    public string? StaIp { get; set; }
    public string? Status { get; set; }
    public string? Ssid { get; set; }
    public int Rssi { get; set; }
}

public class Ws
{
    public bool Connected { get; set; }
}
