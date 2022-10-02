using Microsoft.Extensions.Configuration;

namespace Conectify.Services.Library;

public class Configuration
{
    public Configuration(IConfiguration configuration)
    {
        configuration.Bind(this);
    }

    public string WebsocketUrl { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;

    public Guid DeviceId { get; set; } = Guid.NewGuid();
}