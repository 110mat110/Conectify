namespace Conectify.Services.Dashboard;

public class Configuration(IConfiguration configuration) : Library.ConfigurationBase(configuration)
{
    public Guid SensorId { get; set; }
}
