using Conectify.Services.Library;
using Conectify.Shared.Library.Models.Websocket;

namespace Conectify.Services.ShellyConnector.Services
{
    public interface IShellyService
    {
        Task<bool> SetSwitch(bool isOn);
        Task<bool> SendValueToShelly(Database.Models.Values.Action websocketAction);
    }

    public class ShellyService : IShellyService
    {
        private readonly Configuration configuration;
        private readonly IServicesWebsocketClient websocketClient;
        private readonly ILogger<ShellyService> logger;

        public ShellyService(Configuration configuration, IServicesWebsocketClient websocketClient, ILogger<ShellyService> logger)
        {
            this.configuration = configuration;
            this.websocketClient = websocketClient;
            this.logger = logger;
        }

        public async Task<bool> SetSwitch(bool isOn)
        {
            logger.LogInformation($"Light was turned {LightState(isOn)}");
            var value = new WebsocketValue()
            {
                Name = "Light",
                NumericValue = isOn ? 1 : 100,
                SourceId = configuration.SensorId,
                TimeCreated = DateTime.UtcNow.Ticks,
                Unit = "%",
                Type = "Value",
            };

            await websocketClient.SendMessageAsync(value);

            return true;
        }

        private string LightState(bool isOn)
        {
            return isOn ? "on" : "off";
        }

        public async Task<bool> SendValueToShelly(Database.Models.Values.Action websocketAction)
        {
            if (websocketAction.DestinationId != configuration.ActuatorId)
            {
                return false;
            }

            if (websocketAction.NumericValue > 0)
            {
                using (var client = new HttpClient())
                {
                    logger.LogInformation("Turning on light");
                    var c = await client.GetAsync($"{configuration.ShellyIp}/relay/0?turn=on");
                }
            }
            else
            {
                using (var client = new HttpClient())
                {
                    logger.LogInformation("Turning off light");
                    var c = await client.GetAsync($"{configuration.ShellyIp}/relay/0?turn=off");
                }
            }

            return true;
        }
    }
}
