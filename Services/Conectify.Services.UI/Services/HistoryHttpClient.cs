using Conectify.Shared.Library.Models.Values;
using Newtonsoft.Json;
using System.Text;

namespace Conectify.Services.UI.Services;

public interface IHistoryHttpClient
{
    Task<IEnumerable<Guid>> GetActiveSensorIds(CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetActiveActuatorIds(CancellationToken ct = default);
    Task<Dictionary<Guid, ApiEvent>> GetLatestValuesBatch(IEnumerable<Guid> sensorIds, CancellationToken ct = default);
    Task<IEnumerable<ApiEvent>> GetSensorValues(Guid sensorId, CancellationToken ct = default);
}

public class HistoryHttpClient : IHistoryHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _historyBaseUrl;
    private readonly ILogger<HistoryHttpClient> _logger;

    public HistoryHttpClient(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<HistoryHttpClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _historyBaseUrl = config["HistoryServiceUrl"] ?? "http://localhost:5020";
        _logger = logger;
    }

    public async Task<IEnumerable<Guid>> GetActiveSensorIds(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{_historyBaseUrl}/api/device/sensors", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<IEnumerable<Guid>>(json) ?? [];
    }

    public async Task<IEnumerable<Guid>> GetActiveActuatorIds(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{_historyBaseUrl}/api/device/actuators", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<IEnumerable<Guid>>(json) ?? [];
    }

    public async Task<Dictionary<Guid, ApiEvent>> GetLatestValuesBatch(IEnumerable<Guid> sensorIds, CancellationToken ct = default)
    {
        var ids = sensorIds.ToList();
        var body = JsonConvert.SerializeObject(ids);
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_historyBaseUrl}/api/data/latest-batch", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("latest-batch returned {StatusCode}, falling back to per-sensor latest lookups", response.StatusCode);
            return await GetLatestValuesIndividually(ids, ct);
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<Dictionary<Guid, ApiEvent>>(json) ?? [];
    }

    private async Task<Dictionary<Guid, ApiEvent>> GetLatestValuesIndividually(IEnumerable<Guid> sensorIds, CancellationToken ct)
    {
        var tasks = sensorIds.Select(async id =>
        {
            var response = await _httpClient.GetAsync($"{_historyBaseUrl}/api/data/{id}/latest", ct);
            if (!response.IsSuccessStatusCode)
            {
                return (id, latest: (ApiEvent?)null);
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var latest = JsonConvert.DeserializeObject<ApiEvent>(json);
            return (id, latest);
        });

        var results = await Task.WhenAll(tasks);
        return results
            .Where(r => r.latest is not null)
            .ToDictionary(r => r.id, r => r.latest!);
    }

    public async Task<IEnumerable<ApiEvent>> GetSensorValues(Guid sensorId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{_historyBaseUrl}/api/data/{sensorId}/values", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<IEnumerable<ApiEvent>>(json) ?? [];
    }
}
