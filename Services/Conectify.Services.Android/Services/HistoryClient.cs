using System.Net.Http.Json;
using Conectify.Shared.Library.Models.Values;

namespace Conectify.Services.Android.Services;

/// <summary>
/// Reads current values and the active-device list from the History service, which answers both
/// entirely from its in-memory caches.
///
/// This exists so the widget path never queries the Event table. The previous implementation ran a
/// GroupBy over the whole (unbounded) Event table on every refresh, which is what made the widget
/// time out. Everything here is best-effort: History being down means the widget shows stale or
/// empty values, never an error or a hang.
/// </summary>
public class HistoryClient(HttpClient http, AndroidConfiguration configuration, ILogger<HistoryClient> logger)
{
    private bool IsConfigured => !string.IsNullOrWhiteSpace(configuration.HistoryServiceUrl);

    private string BaseUrl => configuration.HistoryServiceUrl.TrimEnd('/');

    /// <summary>Latest value per source id. Sources with nothing cached are simply absent.</summary>
    public async Task<Dictionary<Guid, ApiEvent>> GetLatestValuesAsync(
        IReadOnlyCollection<Guid> sourceIds, CancellationToken ct = default)
    {
        if (!IsConfigured || sourceIds.Count == 0)
        {
            return [];
        }

        try
        {
            var response = await http.PostAsJsonAsync($"{BaseUrl}/api/data/latest-batch", sourceIds, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Dictionary<Guid, ApiEvent>>(ct) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Latest-value batch failed against {HistoryServiceUrl}", configuration.HistoryServiceUrl);
            return [];
        }
    }

    /// <summary>Sensors and actuators the History service has seen traffic from in the last 24h.</summary>
    public async Task<(HashSet<Guid> Sensors, HashSet<Guid> Actuators)> GetActiveIdsAsync(CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            return ([], []);
        }

        try
        {
            var sensors = http.GetFromJsonAsync<IEnumerable<Guid>>($"{BaseUrl}/api/device/sensors", ct);
            var actuators = http.GetFromJsonAsync<IEnumerable<Guid>>($"{BaseUrl}/api/device/actuators", ct);
            await Task.WhenAll(sensors, actuators);

            return ((await sensors ?? []).ToHashSet(), (await actuators ?? []).ToHashSet());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Active-device lookup failed against {HistoryServiceUrl}", configuration.HistoryServiceUrl);
            return ([], []);
        }
    }
}
