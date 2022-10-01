namespace Conectify.Services.ShellyConnector
{
    public class Configuration : Conectify.Services.Library.Configuration
    {
        public Configuration(IConfiguration configuration) : base(configuration)
        {
        }
        public Guid SensorId { get; set; }

        public Guid ActuatorId { get; set; }

        public string ShellyIp { get; set; } = string.Empty;

    }
}
