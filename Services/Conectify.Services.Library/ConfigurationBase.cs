using Conectify.Shared.Services;
using Microsoft.Extensions.Configuration;

namespace Conectify.Services.Library;

public class ConfigurationBase
{
    public ConfigurationBase(IConfiguration configuration)
    {
        configuration.Bind(this);
    }

    public string WebsocketUrl { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;

    public Guid DeviceId { get; set; } = Guid.NewGuid();

    /// <summary>Settings of the connection watchdog, bound from the "Watchdog" section.</summary>
    public WatchdogConfiguration Watchdog { get; set; } = new();

    private string? externalAddress;

    /// <summary>
    /// The address this service advertises to the Conectify server; the UI turns it into
    /// "http://{address}/" to link to the service. Defaults to the autodetected host address, same
    /// as before. Set it - host, or host:port when the port is not 80 - when that detection is
    /// wrong, which it is under Docker: it returns the container's own 10.x / 172.x address, which
    /// nothing on the LAN can reach.
    /// </summary>
    public string ExternalAddress
    {
        get => string.IsNullOrWhiteSpace(externalAddress) ? AutodetectedAddress.Value : externalAddress;
        set => externalAddress = value;
    }

    // A DNS lookup, and configurations are transient - so resolve it once, and only when asked for.
    private static readonly Lazy<string> AutodetectedAddress = new(WebFunctions.GetIPAdress);
}