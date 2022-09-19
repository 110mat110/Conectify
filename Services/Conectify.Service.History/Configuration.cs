namespace Conectify.Service.History
{
    public class Configuration : Conectify.Services.Library.Configuration
    {
        public Guid SensorId { get; set; }

        public Guid ActuatorId { get; set; }
    }
}
