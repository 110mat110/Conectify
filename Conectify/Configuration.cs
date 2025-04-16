namespace Conectify.Server;

public class Configuration
{
    public Configuration(IConfiguration configuration)
    {
        configuration.Bind(this);
    }

    public string HistoryService { get; set; } = string.Empty;

    public Guid DeviceId { get; set; } = Guid.NewGuid();

    public string GitToken { get; set; } = string.Empty;
}
