namespace Conectify.Services.OccupancyCheck;

public class Configuration : Library.ConfigurationBase
{
    public Configuration(IConfiguration configuration) : base(configuration)
    {
    }
    public Guid SensorId { get; set; }

    public string Password { get; set; } = string.Empty;

    public string IpToSearch { get; set; } = string.Empty;

    public string[] MacAdresses { get; set; } = Array.Empty<string>();
}