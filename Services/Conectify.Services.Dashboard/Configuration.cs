namespace Conectify.Services.Dashboard;

public class Configuration : Library.ConfigurationBase
{
    public Configuration(IConfiguration configuration) : base(configuration)
    {
    }
    public Guid SensorId { get; set; }
}
