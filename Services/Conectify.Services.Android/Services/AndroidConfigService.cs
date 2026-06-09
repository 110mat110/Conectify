using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Services.Android.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Android.Services;

public class AndroidConfigService(ConectifyDb db, ILogger<AndroidConfigService> logger)
{
    public const string WIDGET_LARGE = "large";
    public const string WIDGET_SMALL = "small";

    public async Task<bool> HasConfigAsync(string userMail, string widgetType, CancellationToken ct = default) =>
        await db.AndroidWidgetItems.AnyAsync(i => i.UserMail == userMail && i.WidgetType == widgetType, ct);

    public async Task<(List<Guid> SensorIds, List<Guid> ActuatorIds)> GetOrderedIdsAsync(
        string userMail, string widgetType, CancellationToken ct = default)
    {
        var items = await db.AndroidWidgetItems
            .Where(i => i.UserMail == userMail && i.WidgetType == widgetType)
            .OrderBy(i => i.Position)
            .ToListAsync(ct);

        return (
            items.Where(i => i.SourceType == "sensor").Select(i => i.DeviceId).ToList(),
            items.Where(i => i.SourceType == "actuator").Select(i => i.DeviceId).ToList()
        );
    }

    public async Task SaveConfigAsync(
        string userMail, string widgetType, List<WidgetConfigItemDto> items, CancellationToken ct = default)
    {
        logger.LogInformation("SaveConfig: user={UserMail} widgetType={WidgetType} itemCount={Count}", userMail, widgetType, items.Count);
        var existing = await db.AndroidWidgetItems
            .Where(i => i.UserMail == userMail && i.WidgetType == widgetType)
            .ToListAsync(ct);
        db.AndroidWidgetItems.RemoveRange(existing);

        var newItems = items.Select((item, index) => new AndroidWidgetItem
        {
            UserMail = userMail,
            WidgetType = widgetType,
            DeviceId = item.DeviceId,
            SourceType = item.SourceType,
            Position = index
        });

        await db.AndroidWidgetItems.AddRangeAsync(newItems, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<AvailableDevicesDto> GetAvailableAsync(CancellationToken ct = default)
    {
        var sensors = await db.Sensors
            .Where(s => s.IsKnown)
            .Include(s => s.Metadata)
                .ThenInclude(m => m.Metadata)
            .ToListAsync(ct);

        var actuators = await db.Actuators
            .Where(a => a.IsKnown)
            .Include(a => a.Metadata)
                .ThenInclude(m => m.Metadata)
            .ToListAsync(ct);

        return new AvailableDevicesDto(
            sensors.Select(s => new AvailableDeviceDto(s.Id, DisplayName(s.Name, s.Metadata)))
                   .OrderBy(s => s.Name).ToList(),
            actuators.Select(a => new AvailableDeviceDto(a.Id, DisplayName(a.Name, a.Metadata)))
                     .OrderBy(a => a.Name).ToList()
        );
    }

    private static string DisplayName<T>(string fallback, IEnumerable<Conectify.Database.Models.MetadataConnector<T>> connectors)
    {
        var nameOverride = connectors.FirstOrDefault(m => m.Metadata?.Name == "Name")?.StringValue;
        return !string.IsNullOrWhiteSpace(nameOverride) ? nameOverride : fallback;
    }

    public async Task<List<WidgetConfigItemDto>> GetConfigAsync(
        string userMail, string widgetType, CancellationToken ct = default)
    {
        return await db.AndroidWidgetItems
            .Where(i => i.UserMail == userMail && i.WidgetType == widgetType)
            .OrderBy(i => i.Position)
            .Select(i => new WidgetConfigItemDto(i.DeviceId, i.SourceType))
            .ToListAsync(ct);
    }
}
