using System.Diagnostics;
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
        using var activity = AndroidTelemetry.Source.StartActivity("history.latest_values");
        activity?.SetTag("history.requested_count", sourceIds.Count);

        if (!IsConfigured)
        {
            logger.LogWarning("HistoryServiceUrl is not configured — the widget will show no values");
            activity?.SetTag("history.skipped", "not-configured");
            return [];
        }

        if (sourceIds.Count == 0)
        {
            return [];
        }

        try
        {
            var response = await http.PostAsJsonAsync($"{BaseUrl}/api/data/latest-batch", sourceIds, ct);
            response.EnsureSuccessStatusCode();

            var values = await response.Content.ReadFromJsonAsync<Dictionary<Guid, ApiEvent>>(ct) ?? [];

            activity?.SetTag("history.returned_count", values.Count);
            logger.LogInformation("History returned {ReturnedCount}/{RequestedCount} cached value(s)",
                values.Count, sourceIds.Count);

            return values;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Latest-value batch failed against {HistoryServiceUrl}", configuration.HistoryServiceUrl);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return [];
        }
    }

    /// <summary>Sensors and actuators the History service has seen traffic from in the last 24h.</summary>
    public async Task<(HashSet<Guid> Sensors, HashSet<Guid> Actuators)> GetActiveIdsAsync(CancellationToken ct = default)
    {
        using var activity = AndroidTelemetry.Source.StartActivity("history.active_ids");

        if (!IsConfigured)
        {
            return ([], []);
        }

        try
        {
            var sensors = http.GetFromJsonAsync<IEnumerable<Guid>>($"{BaseUrl}/api/device/sensors", ct);
            var actuators = http.GetFromJsonAsync<IEnumerable<Guid>>($"{BaseUrl}/api/device/actuators", ct);
            await Task.WhenAll(sensors, actuators);

            var result = ((await sensors ?? []).ToHashSet(), (await actuators ?? []).ToHashSet());

            activity?.SetTag("history.active_sensors", result.Item1.Count);
            activity?.SetTag("history.active_actuators", result.Item2.Count);
            logger.LogInformation("History reports {SensorCount} active sensor(s), {ActuatorCount} active actuator(s)",
                result.Item1.Count, result.Item2.Count);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Active-device lookup failed against {HistoryServiceUrl}", configuration.HistoryServiceUrl);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return ([], []);
        }
    }
}
