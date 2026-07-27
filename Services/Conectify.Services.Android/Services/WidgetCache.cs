using Conectify.Database;
using Conectify.Shared.Library;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Android.Services;

public sealed record ThresholdRange(float Min, float Max, string Color);

public sealed record SensorDefinition(Guid Id, string Name, IReadOnlyList<ThresholdRange> Thresholds)
{
    public string AccentColorFor(float? value)
    {
        if (value is null)
        {
            return WidgetCache.DefaultAccentColor;
        }

        foreach (var range in Thresholds)
        {
            if (value >= range.Min && value < range.Max)
            {
                return range.Color;
            }
        }

        return WidgetCache.DefaultAccentColor;
    }
}

public sealed record ActuatorDefinition(
    Guid Id,
    Guid SensorId,
    string Name,
    int IoType,
    float MinVal,
    float MaxVal,
    float TriggerValue);

public sealed record DeviceDefinitions(
    IReadOnlyList<SensorDefinition> Sensors,
    IReadOnlyList<ActuatorDefinition> Actuators);

public sealed record WidgetLayout(IReadOnlyList<Guid> SensorIds, IReadOnlyList<Guid> ActuatorIds);

/// <summary>
/// In-memory snapshot of everything the widget needs that is not a value: which sensors and
/// actuators exist, their display names, their thresholds and IO types, and the per-user ordering.
///
/// These come from small, rarely-changing tables, but they used to be re-queried (with a two-level
/// Include graph) on every single refresh. Here they are loaded once and then served from memory;
/// a stale snapshot is refreshed in the background rather than making the caller wait, so after the
/// first successful load a widget request never blocks on the database.
/// </summary>
public class WidgetCache(IServiceScopeFactory scopeFactory, ILogger<WidgetCache> logger)
{
    public const string DefaultAccentColor = "#4fc3f7";

    private const string MetadataName = "Name";
    private const string MetadataThreshold = "Threshold";
    private const string MetadataIoType = "IOType";

    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    /// <summary>How long to wait before retrying after a failed load, so a dead database is not
    /// hammered once per widget refresh.</summary>
    private static readonly TimeSpan FailureBackoff = TimeSpan.FromSeconds(30);

    private readonly SemaphoreSlim definitionsLock = new(1, 1);
    private volatile DeviceDefinitions? definitions;
    private long definitionsExpireAtTicks;
    private int definitionsRefreshQueued;

    private readonly SemaphoreSlim layoutsLock = new(1, 1);
    private readonly Dictionary<string, (WidgetLayout Layout, DateTime ExpiresAtUtc)> layouts = [];

    // ── Definitions ───────────────────────────────────────────────────────────

    public async Task<DeviceDefinitions> GetDefinitionsAsync(CancellationToken ct = default)
    {
        var snapshot = definitions;

        if (snapshot is null)
        {
            // Cold start only — this is the one request that pays for the database.
            return await LoadDefinitionsAsync(ct);
        }

        // Stale-while-revalidate: the caller gets the current snapshot immediately and the refresh
        // happens behind it. The flag keeps concurrent requests from queueing a task each.
        if (IsExpired() && Interlocked.CompareExchange(ref definitionsRefreshQueued, 1, 0) == 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await LoadDefinitionsAsync(CancellationToken.None);
                }
                finally
                {
                    Interlocked.Exchange(ref definitionsRefreshQueued, 0);
                }
            }, CancellationToken.None);
        }

        return snapshot;
    }

    private bool IsExpired() => DateTime.UtcNow.Ticks >= Interlocked.Read(ref definitionsExpireAtTicks);

    private async Task<DeviceDefinitions> LoadDefinitionsAsync(CancellationToken ct)
    {
        await definitionsLock.WaitAsync(ct);
        try
        {
            // Another caller may have refreshed while we waited.
            if (definitions is not null && !IsExpired())
            {
                return definitions;
            }

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

            // Flat projections rather than Include(): only the six metadata columns that matter
            // cross the wire, and EF does not have to materialise the whole entity graph.
            var sensorRows = await db.Sensors
                .AsNoTracking()
                .Where(s => s.IsKnown)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    Metadata = s.Metadata.Select(m => new MetadataRow(
                        m.Metadata.Name, m.NumericValue, m.StringValue, m.MinVal, m.MaxVal)).ToList()
                })
                .ToListAsync(ct);

            var actuatorRows = await db.Actuators
                .AsNoTracking()
                .Where(a => a.IsKnown)
                .Select(a => new
                {
                    a.Id,
                    a.SensorId,
                    a.Name,
                    Metadata = a.Metadata.Select(m => new MetadataRow(
                        m.Metadata.Name, m.NumericValue, m.StringValue, m.MinVal, m.MaxVal)).ToList()
                })
                .ToListAsync(ct);

            var sensors = sensorRows
                .Where(s => IsVisible(s.Metadata))
                .Select(s => new SensorDefinition(s.Id, DisplayName(s.Name, s.Metadata), Thresholds(s.Metadata)))
                .ToList();

            var actuators = actuatorRows
                .Where(a => IsVisible(a.Metadata))
                .Select(a => BuildActuator(a.Id, a.SensorId, a.Name, a.Metadata))
                .OfType<ActuatorDefinition>()
                .ToList();

            var loaded = new DeviceDefinitions(sensors, actuators);
            definitions = loaded;
            Interlocked.Exchange(ref definitionsExpireAtTicks, DateTime.UtcNow.Add(Ttl).Ticks);

            logger.LogInformation("Widget definitions cached: {SensorCount} sensor(s), {ActuatorCount} actuator(s)",
                sensors.Count, actuators.Count);

            return loaded;
        }
        catch (Exception ex) when (definitions is not null)
        {
            // Keep serving the previous snapshot rather than failing the widget, and back off so the
            // next request does not immediately queue another doomed load.
            Interlocked.Exchange(ref definitionsExpireAtTicks, DateTime.UtcNow.Add(FailureBackoff).Ticks);
            logger.LogError(ex, "Widget definition refresh failed — keeping the previous snapshot");
            return definitions;
        }
        finally
        {
            definitionsLock.Release();
        }
    }

    // ── Layout (per user + widget type) ───────────────────────────────────────

    public async Task<WidgetLayout?> GetLayoutAsync(string userMail, string widgetType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userMail))
        {
            return null;
        }

        var key = LayoutKey(userMail, widgetType);

        if (TryGetFreshLayout(key, out var fresh))
        {
            return fresh;
        }

        await layoutsLock.WaitAsync(ct);
        try
        {
            if (TryGetFreshLayout(key, out fresh))
            {
                return fresh;
            }

            try
            {
                var layout = await LoadLayoutAsync(userMail, widgetType, ct);
                lock (layouts)
                {
                    layouts[key] = (layout, DateTime.UtcNow.Add(Ttl));
                }
                return layout;
            }
            catch (Exception ex)
            {
                // Fall back to the stale entry if we have one; ordering matters far less than the
                // widget rendering at all.
                lock (layouts)
                {
                    if (layouts.TryGetValue(key, out var stale))
                    {
                        layouts[key] = (stale.Layout, DateTime.UtcNow.Add(FailureBackoff));
                        logger.LogError(ex, "Widget layout refresh failed for {UserMail} — using the stale order", userMail);
                        return stale.Layout;
                    }
                }
                throw;
            }
        }
        finally
        {
            layoutsLock.Release();
        }
    }

    private bool TryGetFreshLayout(string key, out WidgetLayout? layout)
    {
        lock (layouts)
        {
            if (layouts.TryGetValue(key, out var cached) && DateTime.UtcNow < cached.ExpiresAtUtc)
            {
                layout = cached.Layout;
                return true;
            }
        }

        layout = null;
        return false;
    }

    /// <summary>Called when a user edits their widget config so the next request re-reads it.</summary>
    public void InvalidateLayout(string userMail, string widgetType)
    {
        lock (layouts)
        {
            layouts.Remove(LayoutKey(userMail, widgetType));
        }
    }

    private async Task<WidgetLayout> LoadLayoutAsync(string userMail, string widgetType, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

        var configured = await db.AndroidWidgetItems
            .AsNoTracking()
            .Where(i => i.UserMail == userMail && i.WidgetType == widgetType)
            .OrderBy(i => i.Position)
            .Select(i => new { i.SourceType, i.DeviceId })
            .ToListAsync(ct);

        if (configured.Count > 0)
        {
            return new WidgetLayout(
                configured.Where(i => i.SourceType == "sensor").Select(i => i.DeviceId).ToList(),
                configured.Where(i => i.SourceType == "actuator").Select(i => i.DeviceId).ToList());
        }

        return await LoadDashboardLayoutAsync(db, userMail, ct);
    }

    /// <summary>Fallback ordering when the user has not configured the widget: their dashboard order.</summary>
    private static async Task<WidgetLayout> LoadDashboardLayoutAsync(ConectifyDb db, string userMail, CancellationToken ct)
    {
        var userId = await db.Users
            .AsNoTracking()
            .Where(u => u.UserMail == userMail)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        if (userId == Guid.Empty)
        {
            return new WidgetLayout([], []);
        }

        var dashboardIds = await db.Dashboards
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.Position)
            .Select(d => d.Id)
            .ToListAsync(ct);

        var allDevices = await db.DashboardsDevice
            .AsNoTracking()
            .Where(dd => dashboardIds.Contains(dd.DashBoardId))
            .Select(dd => new { dd.DashBoardId, dd.SourceType, dd.DeviceId, dd.PosX, dd.PosY })
            .ToListAsync(ct);

        var ordered = dashboardIds
            .SelectMany(dashId => allDevices
                .Where(d => d.DashBoardId == dashId)
                .OrderBy(d => d.PosY).ThenBy(d => d.PosX))
            .ToList();

        return new WidgetLayout(
            ordered.Where(d => d.SourceType == "sensor").Select(d => d.DeviceId).ToList(),
            ordered.Where(d => d.SourceType == "actuator").Select(d => d.DeviceId).ToList());
    }

    private static string LayoutKey(string userMail, string widgetType) => $"{userMail}|{widgetType}";

    // ── Metadata interpretation ───────────────────────────────────────────────

    private sealed record MetadataRow(string Name, float? NumericValue, string StringValue, float? MinVal, float? MaxVal);

    private static bool IsVisible(List<MetadataRow> metadata)
    {
        var visible = metadata.FirstOrDefault(m => m.Name == Constants.Metadatas.Visible);
        return visible is null || (visible.NumericValue ?? 0) > 0;
    }

    private static string DisplayName(string fallback, List<MetadataRow> metadata)
    {
        var nameOverride = metadata.FirstOrDefault(m => m.Name == MetadataName)?.StringValue;
        return string.IsNullOrWhiteSpace(nameOverride) ? fallback : nameOverride;
    }

    private static List<ThresholdRange> Thresholds(List<MetadataRow> metadata) =>
        metadata
            .Where(m => m.Name == MetadataThreshold
                        && m.MinVal is not null && m.MaxVal is not null
                        && m.StringValue.StartsWith('#'))
            .Select(m => new ThresholdRange(m.MinVal!.Value, m.MaxVal!.Value, m.StringValue))
            .ToList();

    /// <summary>Null when the actuator has no usable IOType — those are not shown in the widget.</summary>
    private static ActuatorDefinition? BuildActuator(Guid id, Guid sensorId, string name, List<MetadataRow> metadata)
    {
        var ioMeta = metadata.FirstOrDefault(m => m.Name == MetadataIoType);
        if (ioMeta is null)
        {
            return null;
        }

        var ioType = ParseIoType(ioMeta.StringValue);
        if (ioType is null or -1)
        {
            return null;
        }

        return new ActuatorDefinition(
            Id: id,
            SensorId: sensorId,
            Name: DisplayName(name, metadata),
            IoType: ioType.Value,
            MinVal: ioMeta.MinVal ?? 0f,
            MaxVal: ioMeta.MaxVal ?? 1f,
            TriggerValue: ioMeta.NumericValue ?? 1f);
    }

    private static int? ParseIoType(string value) => value switch
    {
        "Linear" => 0,
        "Binary" => 1,
        "Trigger" => 2,
        "Color" => 3,
        "CCT" => 4,
        "Undefined" => -1,
        _ => null,
    };
}
