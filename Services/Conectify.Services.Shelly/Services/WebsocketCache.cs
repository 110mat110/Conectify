using Conectify.Services.Shelly.Models.Shelly;
using System.Net.WebSockets;

namespace Conectify.Services.Shelly.Services;

public class WebsocketCache
{

    public Dictionary<string, ShellyDeviceCacheItem> Cache = [];
}

public class ShellyDeviceCacheItem
{
    public string ShellyId { get; set; }
    public WebSocket WebSocket { get; set; }
    public IShelly Shelly { get; set; }
}