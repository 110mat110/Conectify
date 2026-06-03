using Conectify.Services.Library;

namespace Conectify.Services.Android;

public class AndroidConfiguration(IConfiguration configuration) : ConfigurationBase(configuration)
{
    public Guid SensorId { get; set; }
    public Guid ActuatorId { get; set; }
    public string HistoryServiceUrl { get; set; } = string.Empty;
}
