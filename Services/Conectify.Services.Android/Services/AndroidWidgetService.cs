using Conectify.Database;
using Conectify.Services.Android.Models;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Android.Services;

/// <summary>
/// Builds the widget payload. A refresh reads device definitions from <see cref="WidgetCache"/> and
/// current values from the History service's in-memory cache — no table scans, and after warm-up no
/// database access at all on the refresh path.
/// </summary>
public class AndroidWidgetService(
    ConectifyDb db,
    WidgetCache cache,
    HistoryClient history,
    IServicesWebsocketClient websocketClient,
    AndroidConfiguration configuration,
    ILogger<AndroidWidgetService> logger)
{
    public async Task<WidgetDataDto> GetWidgetDataAsync(
        string userMail, string widgetType = AndroidConfigService.WIDGET_LARGE, CancellationToken ct = default)
    {
        var definitions = await cache.GetDefinitionsAsync(ct);
        var layout = await cache.GetLayoutAsync(userMail, widgetType, ct);

        var sensorOrder = layout?.SensorIds;
        var actuatorOrder = layout?.ActuatorIds;

        // The active-device lists are only the fallback ordering, so skip those two calls entirely
        // when the user has a configured or dashboard-derived order — which is the normal case.
        HashSet<Guid>? activeSensors = null;
        HashSet<Guid>? activeActuators = null;
        if (sensorOrder is not { Count: > 0 } || actuatorOrder is not { Count: > 0 })
        {
            (activeSensors, activeActuators) = await history.GetActiveIdsAsync(ct);
        }

        var sensors = Select(definitions.Sensors, s => s.Id, sensorOrder, activeSensors, fallbackTake: 5);
        var actuators = Select(definitions.Actuators, a => a.Id, actuatorOrder, activeActuators, fallbackTake: null);

        // One round trip for every value on the widget. An actuator's value lives on its linked
        // sensor, so both kinds of id go in the same request.
        var valueIds = sensors.Select(s => s.Id)
            .Concat(actuators.Select(a => a.SensorId))
            .Distinct()
            .ToList();

        var values = await history.GetLatestValuesAsync(valueIds, ct);

        return new WidgetDataDto(
            sensors.Select(s => ToDto(s, Value(values, s.Id))).ToList(),
            actuators.Select(a => ToDto(a, Value(values, a.SensorId))).ToList());
    }

    /// <summary>
    /// Sets a new value for an actuator (a button press in the widget). Returns false only when the
    /// actuator does not exist at all.
    /// </summary>
    public async Task<bool> SetActuatorValueAsync(Guid actuatorId, SetActuatorValueDto dto, CancellationToken ct = default)
    {
        if (!await ActuatorExistsAsync(actuatorId, ct))
        {
            logger.LogWarning("SetActuatorValue: actuator {ActuatorId} not found", actuatorId);
            return false;
        }

        var ev = new WebsocketEvent
        {
            Id = Guid.NewGuid(),
            SourceId = configuration.SensorId,
            Type = Constants.Events.Action,
            Name = "Android widget",
            NumericValue = dto.NumericValue,
            StringValue = dto.StringValue ?? string.Empty,
            Unit = dto.Unit,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            DestinationId = actuatorId
        };

        await websocketClient.SendMessageAsync(ev, ct);

        logger.LogInformation(
            "SetActuatorValue: sent event {EventId} to actuator {ActuatorId} with numericValue={NumericValue}",
            ev.Id, actuatorId, ev.NumericValue);

        return true;
    }

    /// <summary>
    /// Cache first; the primary-key lookup is only reached for an actuator created since the last
    /// cache refresh, or one the widget filters out for having no IOType.
    /// </summary>
    private async Task<bool> ActuatorExistsAsync(Guid actuatorId, CancellationToken ct)
    {
        var definitions = await cache.GetDefinitionsAsync(ct);
        return definitions.Actuators.Any(a => a.Id == actuatorId)
            || await db.Actuators.AsNoTracking().AnyAsync(a => a.Id == actuatorId, ct);
    }

    // ── Selection ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Explicit order wins; otherwise show what History reports as active; otherwise fall back to
    /// everything (capped by <paramref name="fallbackTake"/> where a cap makes sense).
    /// </summary>
    private static List<T> Select<T>(
        IReadOnlyList<T> all,
        Func<T, Guid> idOf,
        IReadOnlyList<Guid>? order,
        HashSet<Guid>? activeIds,
        int? fallbackTake)
    {
        if (order is { Count: > 0 })
        {
            var byId = all.ToDictionary(idOf);
            return order.Where(byId.ContainsKey).Select(id => byId[id]).ToList();
        }

        if (activeIds is { Count: > 0 })
        {
            return all.Where(x => activeIds.Contains(idOf(x))).ToList();
        }

        return fallbackTake is null ? all.ToList() : all.Take(fallbackTake.Value).ToList();
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static ApiEvent? Value(Dictionary<Guid, ApiEvent> values, Guid id) =>
        values.TryGetValue(id, out var value) ? value : null;

    private static WidgetSensorDto ToDto(SensorDefinition sensor, ApiEvent? value) => new(
        Id: sensor.Id,
        Name: sensor.Name,
        NumericValue: value?.NumericValue,
        StringValue: value?.StringValue ?? string.Empty,
        Unit: value?.Unit ?? string.Empty,
        TimeCreated: value?.TimeCreated ?? 0,
        AccentColor: sensor.AccentColorFor(value?.NumericValue));

    private static WidgetActuatorDto ToDto(ActuatorDefinition actuator, ApiEvent? value) => new(
        Id: actuator.Id,
        Name: actuator.Name,
        IsActive: (value?.NumericValue ?? 0f) > 0f,
        CurrentValue: value?.NumericValue,
        Unit: value?.Unit ?? string.Empty,
        IoType: actuator.IoType,
        MinVal: actuator.MinVal,
        MaxVal: actuator.MaxVal,
        TriggerValue: actuator.TriggerValue);
}
