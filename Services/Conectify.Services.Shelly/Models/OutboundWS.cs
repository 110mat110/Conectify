using Newtonsoft.Json;

namespace Conectify.Services.Shelly.Models;

public class OutboundWS
{
    public string jsonrpc = "2.0";
    public int id = 10;
    public string src = "wsserver";
    public string method { get; set; }
    [JsonProperty("params")]
    public object Params { get; set; }
}
