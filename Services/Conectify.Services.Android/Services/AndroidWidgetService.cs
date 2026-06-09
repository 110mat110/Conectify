using System.Text.Json;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Services.Android.Models;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Android.Services;

public class AndroidWidgetService(
    ConectifyDb db,
    AndroidConfigService configService,
    IServicesWebsocketClient websocketClient,
    AndroidConfiguration configuration,
    IHttpFactory httpFactory,
    ILogger<AndroidWidgetService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // ── 1. Full widget data ───────────────────────────────────────────────────

    public async Task<WidgetDataDto> GetWidgetDataAsync(
        string userMail, string widgetType = AndroidConfigService.WIDGET_LARGE, CancellationToken ct = default)
    {
        var (activeSensorIds, activeActuatorIds) = await GetActiveIdsAsync(ct);

        List<Guid>? sensorOrder = null;
        List<Guid>? actuatorOrder = null;

        if (!string.IsNullOrWhiteSpace(userMail))
        {
            if (await configService.HasConfigAsync(userMail, widgetType, ct))
            {
                // Use explicitly configured order
                (sensorOrder, actuatorOrder) = await configService.GetOrderedIdsAsync(userMail, widgetType, ct);
            }
            else
            {
                // Fall back to dashboard order
                (sensorOrder, actuatorOrder) = await GetDashboardOrderAsync(userMail, ct);
            }
        }

        var sensors = await GetSensorsAsync(activeSensorIds, sensorOrder, ct);
        var actuators = await GetActuatorsAsync(activeActuatorIds, actuatorOrder, ct);
        return new WidgetDataDto(sensors, actuators);
    }

    // ── Dashboard order ───────────────────────────────────────────────────────

    private async Task<(List<Guid> sensors, List<Guid> actuators)> GetDashboardOrderAsync(string userMail, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserMail == userMail, ct);
        if (user is null) return ([], []);

        var dashboardIds = await db.Dashboards
            .Where(d => d.UserId == user.Id)
            .OrderBy(d => d.Position)
            .Select(d => d.Id)
            .ToListAsync(ct);

        // Load devices per dashboard in page order, then sort by position within each page
        var allDevices = await db.DashboardsDevice
            .Where(dd => dashboardIds.Contains(dd.DashBoardId))
            .ToListAsync(ct);

        var devices = dashboardIds
            .SelectMany(dashId => allDevices
                .Where(d => d.DashBoardId == dashId)
                .OrderBy(d => d.PosY).ThenBy(d => d.PosX))
            .ToList();

        return (
            devices.Where(d => d.SourceType == "sensor").Select(d => d.DeviceId).ToList(),
            devices.Where(d => d.SourceType == "actuator").Select(d => d.DeviceId).ToList()
        );
    }

    // ── Active IDs from History service ──────────────────────────────────────

    private async Task<(HashSet<Guid> sensors, HashSet<Guid> actuators)> GetActiveIdsAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(configuration.HistoryServiceUrl))
            return ([], []);

        var client = httpFactory.HttpClient;
        client.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            var baseUrl = configuration.HistoryServiceUrl.TrimEnd('/');

            var sensorsJson = await client.GetStringAsync($"{baseUrl}/api/device/sensors", ct);
            var actuatorsJson = await client.GetStringAsync($"{baseUrl}/api/device/actuators", ct);

            var sensorIds = JsonSerializer.Deserialize<IEnumerable<Guid>>(sensorsJson, JsonOptions) ?? [];
            var actuatorIds = JsonSerializer.Deserialize<IEnumerable<Guid>>(actuatorsJson, JsonOptions) ?? [];

            return (sensorIds.ToHashSet(), actuatorIds.ToHashSet());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "History service unavailable at {HistoryServiceUrl}", configuration.HistoryServiceUrl);
            return ([], []);
        }
    }

    // ── Sensors ───────────────────────────────────────────────────────────────

    private async Task<List<WidgetSensorDto>> GetSensorsAsync(
        HashSet<Guid> activeIds, List<Guid>? dashboardOrder, CancellationToken ct)
    {
        var targetIds = dashboardOrder ?? (activeIds.Count > 0 ? activeIds.ToList() : null);

        var query = db.Sensors
            .Where(s => s.IsKnown)
            .Include(s => s.SourceDevice)
            .Include(s => s.Metadata)
                .ThenInclude(m => m.Metadata);

        var sensors = targetIds is { Count: > 0 }
            ? await query.Where(s => targetIds.Contains(s.Id)).ToListAsync(ct)
            : await query.Take(5).ToListAsync(ct);

        var visible = sensors.Where(s => IsVisible(s.Metadata)).ToList();

        // Apply dashboard order if available, otherwise keep DB order
        if (dashboardOrder is { Count: > 0 })
        {
            var map = visible.ToDictionary(s => s.Id);
            visible = dashboardOrder.Where(map.ContainsKey).Select(id => map[id]).ToList();
        }

        var lastValues = await LastValuesForAsync(visible.Select(s => s.Id).ToList(), ct);

        return visible.Select(s =>
        {
            lastValues.TryGetValue(s.Id, out var val);
            return new WidgetSensorDto(
                Id: s.Id,
                Name: DisplayName(s.Name, s.Metadata),
                SourceDeviceName: s.SourceDevice?.Name ?? string.Empty,
                CurrentNumericValue: val?.NumericValue,
                CurrentStringValue: val?.StringValue ?? string.Empty,
                Unit: val?.Unit ?? string.Empty,
                LastUpdated: val?.TimeCreated ?? 0,
                Metadata: MapMetadata(s.Metadata)
            );
        }).ToList();
    }

    // ── Actuators ─────────────────────────────────────────────────────────────

    private async Task<List<WidgetActuatorDto>> GetActuatorsAsync(
        HashSet<Guid> activeIds, List<Guid>? dashboardOrder, CancellationToken ct)
    {
        var targetIds = dashboardOrder ?? (activeIds.Count > 0 ? activeIds.ToList() : null);

        var query = db.Actuators
            .Where(a => a.IsKnown)
            .Include(a => a.SourceDevice)
            .Include(a => a.Metadata)
                .ThenInclude(m => m.Metadata);

        var actuators = targetIds is { Count: > 0 }
            ? await query.Where(a => targetIds.Contains(a.Id)).ToListAsync(ct)
            : await query.ToListAsync(ct);

        var visible = actuators.Where(a => IsVisible(a.Metadata)).ToList();

        if (dashboardOrder is { Count: > 0 })
        {
            var map = visible.ToDictionary(a => a.Id);
            visible = dashboardOrder.Where(map.ContainsKey).Select(id => map[id]).ToList();
        }

        var linkedSensorIds = visible.Select(a => a.SensorId).Distinct().ToList();
        var lastValues = await LastValuesForAsync(linkedSensorIds, ct);

        return visible.Select(a =>
        {
            lastValues.TryGetValue(a.SensorId, out var val);
            return new WidgetActuatorDto(
                Id: a.Id,
                Name: DisplayName(a.Name, a.Metadata),
                SourceDeviceName: a.SourceDevice?.Name ?? string.Empty,
                CurrentNumericValue: val?.NumericValue,
                CurrentStringValue: val?.StringValue ?? string.Empty,
                Unit: val?.Unit ?? string.Empty,
                LastUpdated: val?.TimeCreated ?? 0,
                Metadata: MapMetadata(a.Metadata)
            );
        }).ToList();
    }

    // ── 2. Refresh — current values only ──────────────────────────────────────

    public async Task<List<CurrentValueDto>> GetCurrentValuesAsync(CancellationToken ct = default)
    {
        var sensorIds = await db.Sensors.Where(s => s.IsKnown).Take(5).Select(s => s.Id).ToListAsync(ct);

        var actuatorSensorIds = await db.Actuators
            .Where(a => a.IsKnown)
            .Select(a => new { a.Id, a.SensorId })
            .ToListAsync(ct);

        var allSourceIds = sensorIds
            .Concat(actuatorSensorIds.Select(a => a.SensorId))
            .Distinct()
            .ToList();

        var lastValues = await LastValuesForAsync(allSourceIds, ct);

        var result = lastValues.Values.Select(v => new CurrentValueDto(
            Id: v.SourceId,
            NumericValue: v.NumericValue,
            StringValue: v.StringValue,
            Unit: v.Unit,
            TimeCreated: v.TimeCreated
        )).ToList();

        foreach (var a in actuatorSensorIds)
        {
            if (lastValues.TryGetValue(a.SensorId, out var val))
            {
                result.Add(new CurrentValueDto(
                    Id: a.Id,
                    NumericValue: val.NumericValue,
                    StringValue: val.StringValue,
                    Unit: val.Unit,
                    TimeCreated: val.TimeCreated
                ));
            }
        }

        return result;
    }

    // ── 3. Set actuator value via WebSocket ───────────────────────────────────

    public async Task<bool> SetActuatorValueAsync(Guid actuatorId, SetActuatorValueDto dto, CancellationToken ct = default)
    {
        var actuator = await db.Actuators.FirstOrDefaultAsync(a => a.Id == actuatorId, ct);
        if (actuator is null)
        {
            logger.LogWarning("SetActuatorValue: actuator {ActuatorId} not found in database", actuatorId);
            return false;
        }

        logger.LogInformation("SetActuatorValue: found actuator {ActuatorId} name={ActuatorName} sourceDevice={SourceDeviceId}",
            actuatorId, actuator.Name, actuator.SourceDeviceId);

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

        logger.LogInformation(
            "SetActuatorValue: sending websocket event {EventId} destinationId={DestinationId} sourceId={SourceId} numericValue={NumericValue} stringValue={StringValue} unit={Unit}",
            ev.Id, ev.DestinationId, ev.SourceId, ev.NumericValue, ev.StringValue, ev.Unit);

        await websocketClient.SendMessageAsync(ev, ct);

        logger.LogInformation("SetActuatorValue: websocket event {EventId} sent successfully", ev.Id);
        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Dictionary<Guid, Event>> LastValuesForAsync(List<Guid> ids, CancellationToken ct) =>
        await db.Events
            .Where(e => ids.Contains(e.SourceId))
            .GroupBy(e => e.SourceId)
            .Select(g => g.OrderByDescending(e => e.TimeCreated).First())
            .ToDictionaryAsync(e => e.SourceId, ct);

    private static bool IsVisible<T>(IEnumerable<Database.Models.MetadataConnector<T>> connectors)
    {
        var meta = connectors.FirstOrDefault(m => m.Metadata?.Name == Constants.Metadatas.Visible);
        return meta is null || (meta.NumericValue ?? 0) > 0;
    }

    private static string DisplayName<T>(string fallback, IEnumerable<Database.Models.MetadataConnector<T>> connectors)
    {
        var nameOverride = connectors
            .FirstOrDefault(m => m.Metadata?.Name == "Name")
            ?.StringValue;
        return !string.IsNullOrWhiteSpace(nameOverride) ? nameOverride : fallback;
    }

    private static List<MetadataDto> MapMetadata<T>(IEnumerable<Database.Models.MetadataConnector<T>> connectors) =>
        connectors.Select(m => new MetadataDto(
            Name: m.Metadata?.Name ?? string.Empty,
            NumericValue: m.NumericValue,
            StringValue: m.StringValue,
            Unit: m.Unit,
            MinVal: m.MinVal,
            MaxVal: m.MaxVal
        )).ToList();
}
