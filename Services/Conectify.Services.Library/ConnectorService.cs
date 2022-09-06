using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Library.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Conectify.Services.Library
{
    public interface IConnectorService
    {
        Task<bool> RegisterDevice(ApiDevice device, IEnumerable<ApiSensor> apiSensors, IEnumerable<ApiActuator> apiActuators);
    }

    internal class ConnectorService : IConnectorService
    {
        private readonly ILogger<ConnectorService> logger;
        private readonly Configuration configuration;

        public ConnectorService(ILogger<ConnectorService> logger, Configuration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task<bool> RegisterDevice(ApiDevice device, IEnumerable<ApiSensor> apiSensors, IEnumerable<ApiActuator> apiActuators)//TODO better naming for function
        {
            await PostAsync(device, "{0}/api/device/");

            foreach (var apiSensor in apiSensors)
            {
                await PostAsync(apiSensor, "{0}/api/sensors");
            }

            foreach (var apiActuator in apiActuators)
            {
                await PostAsync(apiActuator, "{0}/api/actuators");
            }

            return true;
        }

        private async Task PostAsync<T>(T objectToSend, string urlSuffix) where T : IApiModel
        {
            var finalURL = string.Format(urlSuffix, configuration.ServerUrl);
            var serializedApiModel = JsonConvert.SerializeObject(objectToSend);
            finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");
            logger.LogInformation($"Post to: {finalURL} with content; {serializedApiModel}");

            using HttpClient client = new();
            var content = new StringContent(serializedApiModel, Encoding.UTF8, "application/json");
            await client.PostAsync(finalURL, content);
            //Todo return result!
        }
    }
}
