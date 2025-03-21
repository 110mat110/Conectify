using AutoMapper;
using Conectify.Database.Models;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Conectify.Services.Library;

public interface IConnectorService
{
    Task<bool> RegisterDevice(ApiDevice device, IEnumerable<ApiSensor> apiSensors, IEnumerable<ApiActuator> apiActuators, CancellationToken ct = default);

    Task<bool> RegisterSensors(IEnumerable<ApiSensor> apiSensors, CancellationToken ct = default);

    Task<bool> RegisterActuators(IEnumerable<ApiActuator> apiSensors, CancellationToken ct = default);

    Task<bool> SetPreferences(Guid deviceId, IEnumerable<ApiPreference> preferences, CancellationToken ct = default);

    Task<IEnumerable<Actuator>> LoadAllActuators(CancellationToken ct = default);

    Task<ApiActuator?> LoadActuator(Guid id, CancellationToken ct = default);

    Task<ApiSensor?> LoadSensor(Guid id, CancellationToken ct = default);

    Task<bool> SendMetadataForDevice(Guid deviceId, IEnumerable<MetadataServiceConnector> metadatas, CancellationToken cancellationToken = default);

    Task<IEnumerable<Actuator>> LoadActuatorsPerDevice(Guid deviceId, CancellationToken ct = default);

    Task<IEnumerable<Sensor>> LoadSensorsPerDevice(Guid deviceId, CancellationToken ct = default);

    Task<ApiEvent?> LoadLastValue(Guid sensorId, CancellationToken ct = default);
    Task SendMetadataForSensor(List<Tuple<Guid, MetadataServiceConnector>> metadatas, CancellationToken ct = default);
}

public class ConnectorService(ILogger<ConnectorService> logger, ConfigurationBase configuration, IMapper mapper, IHttpFactory httpProvider) : IConnectorService
{
    public async Task<bool> RegisterDevice(ApiDevice device, IEnumerable<ApiSensor> apiSensors, IEnumerable<ApiActuator> apiActuators, CancellationToken ct = default)
    {
        await PostAsync(device, "{0}/api/device", ct);
        await RegisterSensors(apiSensors, ct);
        await RegisterActuators(apiActuators, ct);
        return true;
    }

    public async Task<bool> RegisterSensors(IEnumerable<ApiSensor> apiSensors, CancellationToken ct = default)
    {
        foreach (var apiSensor in apiSensors)
        {
            await PostAsync(apiSensor, "{0}/api/sensors", ct);
        }

        return true;
    }

    public async Task<bool> RegisterActuators(IEnumerable<ApiActuator> apiActuators, CancellationToken ct = default)
    {
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

    public async Task<bool> SendMetadataForDevice(Guid deviceId, IEnumerable<MetadataServiceConnector> metadatas, CancellationToken cancellationToken = default)
    {
        if (!metadatas.Any())
        {
            return false;
        }

        var allMetadata = await LoadAllMetadata(cancellationToken);
        if (allMetadata is null || !allMetadata.Any() || !metadatas.Any(x => allMetadata.Select(x => x.Name.ToLowerInvariant()).Contains(x.MetadataName.ToLowerInvariant())))
        {
            return false;
        }


        foreach (var metadata in metadatas.Where(x => allMetadata.Select(x => x.Name).Contains(x.MetadataName)))
        {
            var apiModel = mapper.Map<ApiMetadataConnector>(metadata);

            apiModel.DeviceId = deviceId;
            apiModel.MetadataId = allMetadata.First(x => x.Name.ToLowerInvariant() == metadata.MetadataName.ToLowerInvariant()).Id;

            await PostAsync(apiModel, "{0}/api/device/metadata", cancellationToken);
        }

        return true;
    }

    public async Task<IEnumerable<Actuator>> LoadAllActuators(CancellationToken ct = default)
    {
        var apiModels = await GetAsync<IEnumerable<ApiActuator>>("{0}/api/actuators/all", ct);
        return mapper.Map<IEnumerable<Actuator>>(apiModels);
    }

    public async Task<ApiActuator?> LoadActuator(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<ApiActuator>("{0}/api/actuators/" + id.ToString(), ct);
    }

    public async Task<ApiSensor?> LoadSensor(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<ApiSensor>("{0}/api/sensors/" + id.ToString(), ct);
    }

    private async Task<IEnumerable<ApiBasicMetadata>?> LoadAllMetadata(CancellationToken ct = default)
    {
        return await GetAsync<IEnumerable<ApiBasicMetadata>>("{0}/api/metadata/all", ct);
    }

    public async Task<IEnumerable<Actuator>> LoadActuatorsPerDevice(Guid deviceId, CancellationToken ct = default)
    {
        var apiModels = await GetAsync<IEnumerable<ApiActuator>>("{0}/api/actuators/by-device/" + deviceId.ToString(), ct);
        return mapper.Map<IEnumerable<Actuator>>(apiModels);
    }

    public async Task<IEnumerable<Sensor>> LoadSensorsPerDevice(Guid deviceId, CancellationToken ct = default)
    {
        var apiModels = await GetAsync<IEnumerable<ApiSensor>>("{0}/api/sensors/by-device/" + deviceId.ToString(), ct);
        return mapper.Map<IEnumerable<Sensor>>(apiModels);
    }

    public async Task<ApiEvent?> LoadLastValue(Guid sensorId, CancellationToken ct = default)
    {
        return await GetAsync<ApiEvent>("{0}/api/sensors/lastValue/" + sensorId.ToString(), ct);
    }

    private async Task PostAsync<T>(T objectToSend, string urlSuffix, CancellationToken ct = default) where T : IApiModel
    {
        var finalURL = string.Format(urlSuffix, configuration.ServerUrl);
        var serializedApiModel = JsonConvert.SerializeObject(objectToSend);
        finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");
        logger.LogInformation("Post to: {finalURL} with content; {serializedApiModel}", finalURL, serializedApiModel);

        using var client = httpProvider.HttpClient;
        var message = new HttpRequestMessage(HttpMethod.Post, finalURL)
        {
            Content = new StringContent(serializedApiModel, Encoding.UTF8, "application/json")
        };
        await client.SendAsync(message, ct);
    }

    private async Task<TResult?> PostAsync<T, TResult>(T objectToSend, string urlSuffix, CancellationToken ct = default) where T : IApiModel
    {
        var finalURL = string.Format(urlSuffix, configuration.ServerUrl);
        var serializedApiModel = JsonConvert.SerializeObject(objectToSend);
        finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");
        logger.LogInformation("Post to: {finalURL} with content; {serializedApiModel}", finalURL, serializedApiModel);

        using var client = httpProvider.HttpClient;
        var message = new HttpRequestMessage(HttpMethod.Post, finalURL)
        {
            Content = new StringContent(serializedApiModel, Encoding.UTF8, "application/json")
        };
        var result = await client.SendAsync(message, ct);
        var jsonResult = await result.Content.ReadAsStringAsync(ct);

        return JsonConvert.DeserializeObject<TResult>(jsonResult);
    }

    private async Task<T?> GetAsync<T>(string urlSuffix, CancellationToken ct = default)
    {
        var finalURL = string.Format(urlSuffix, configuration.ServerUrl);
        finalURL = finalURL.Replace("//", "/").Replace(@"\\", @"\").Replace("http:/", "http://").Replace("https:/", "https://");
        logger.LogInformation("Get from: {finalURL}", finalURL);

        using var client = httpProvider.HttpClient;
        var result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, finalURL), ct);
        var jsonResult = await result.Content.ReadAsStringAsync(ct);

        return JsonConvert.DeserializeObject<T>(jsonResult);
    }

    public async Task SendMetadataForSensor(List<Tuple<Guid, MetadataServiceConnector>> metadatas, CancellationToken cancellationToken = default)
    {
        var allMetadata = await LoadAllMetadata(cancellationToken);
        if (allMetadata is null || !allMetadata.Any())
        {
            logger.LogError("Cannot load metadatas!");
            return;
        }

        foreach (var metadata in metadatas)
        {
            var metadataId = allMetadata.FirstOrDefault(x => x.Name.ToLowerInvariant() == metadata.Item2.MetadataName.ToLowerInvariant())?.Id;

            if (metadataId is null)
            {
                logger.LogWarning($"Cannot find metadata {metadata.Item2.MetadataName}");
                continue;
            }

            var apiModel = mapper.Map<ApiMetadataConnector>(metadata.Item2);

            apiModel.DeviceId = metadata.Item1;
            apiModel.MetadataId = metadataId.Value;

            await PostAsync(apiModel, "{0}/api/sensors/metadata", cancellationToken);
        }
    }
}
