using Conectify.Services.Library;

namespace Conectify.Services.Automatization
{
    public class AutomatizationConfiguration : Configuration
    {
        public AutomatizationConfiguration(IConfiguration configuration) :base(configuration)
        {

        }
        public int RefreshIntervalSeconds { get; set; } = 1;

        public Guid SensorId { get; set; }

        public Guid ActuatorId { get; set; }
    }
}
