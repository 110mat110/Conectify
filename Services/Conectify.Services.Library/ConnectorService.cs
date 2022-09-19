using AutoMapper;
using Conectify.Database.Models;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Library.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Conectify.Services.Library
{
    public interface IConnectorService
    {
        Task<bool> RegisterDevice(ApiDevice device, IEnumerable<ApiSensor> apiSensors, IEnumerable<ApiActuator> apiActuators, CancellationToken ct = default);

        Task<bool> SetPreferences(Guid deviceId, IEnumerable<ApiPreference> preferences, CancellationToken ct = default);

        Task<IEnumerable<Actuator>> LoadAllActuators(CancellationToken ct = default);
    }

    internal class ConnectorService : IConnectorService
    {
        private readonly ILogger<ConnectorService> logger;
        private readonly Configuration configuration;
        private readonly IMapper mapper;

        public ConnectorService(ILogger<ConnectorService> logger, Configuration configuration, IMapper mapper)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        public async Task<bool> RegisterDevice(ApiDevice device, IEnumerable<ApiSensor> apiSensors, IEnumerable<ApiActuator> apiActuators, CancellationToken ct = default)//TODO better naming for function
        {
            await PostAsync(device, "{0}/api/device/", ct);

            foreach (var apiSensor in apiSensors)
            {
                await PostAsync(apiSensor, "{0}/api/sensors", ct);
            }

            foreach (var apiActuator in apiActuators)
            {
                await PostAsync(apiActuator, "{0}/api/actuators", ct);
            }

            return true;
        }

        public async Task<bool> SetPreferences(Guid deviceId, IEnumerable<ApiPreference> preferences, CancellationToken ct = default)
        {
            if (!preferences.Any())
            {
                return false;
            }

            await PostAsync(new ApiPreferences() { Preferences = preferences }, "{0}/api/subscribe/" + deviceId.ToString(), ct);

            return true;
        }

        public async Task<IEnumerable<Actuator>> LoadAllActuators(CancellationToken ct = default)
        {
            var apiModels = await GetAsync<IEnumerable<ApiActuator>>("{0}/api/actuators/all", ct);
            return mapper.Map<IEnumerable<Actuator>>(apiModels);
        }

        private async Task PostAsync<T>(T objectToSend, string urlSuffix, CancellationToken ct = default) where T : IApiModel
        {
            var finalURL = string.Format(urlSuffix, configuration.ServerUrl);
            var serializedApiModel = JsonConvert.SerializeObject(objectToSend);
            finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");
            logger.LogInformation($"Post to: {finalURL} with content; {serializedApiModel}");

            using HttpClient client = new();
            var content = new StringContent(serializedApiModel, Encoding.UTF8, "application/json");
            await client.PostAsync(finalURL, content, ct);
            //Todo return result!
        }

        private async Task<T?> GetAsync<T>(string urlSuffix, CancellationToken ct = default)
        {
            var finalURL = string.Format(urlSuffix, configuration.ServerUrl);
            finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");
            logger.LogInformation($"Get to: {finalURL}");

            using HttpClient client = new();
            var result = await client.GetAsync(finalURL, ct);
            var jsonResult = await result.Content.ReadAsStringAsync(ct);

            return JsonConvert.DeserializeObject<T>(jsonResult);
        }
    }
}
