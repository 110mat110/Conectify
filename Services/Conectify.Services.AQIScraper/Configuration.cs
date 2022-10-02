namespace Conectify.Services.AQIScraper;

public class Configuration : Conectify.Services.Library.Configuration
{
    public Configuration(IConfiguration configuration) : base(configuration)
    {
    }
    public Guid SensorId { get; set; }
}