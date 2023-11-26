namespace Conectify.Services.AQIScraper;

public class Configuration : Library.ConfigurationBase
{
    public Configuration(IConfiguration configuration) : base(configuration)
    {
    }
    public Guid SensorId { get; set; }
}