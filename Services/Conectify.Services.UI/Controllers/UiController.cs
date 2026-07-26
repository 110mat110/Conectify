using Conectify.Services.UI.Models;
using Conectify.Services.UI.Services;
using Conectify.Shared.Library.Models.Values;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.UI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UiController(
    IDeviceMetadataCachingService metadataCache,
    IHistoryHttpClient historyClient,
    ILogger<UiController> logger) : ControllerBase
{
    [HttpGet("sensors")]
    public async Task<IEnumerable<UiSensorResponse>> GetSensors(CancellationToken ct)
    {
        var activeSensorIds = (await historyClient.GetActiveSensorIds(ct)).ToList();
        if (activeSensorIds.Count == 0) return [];

        // One call to history: latest value + downsampled 24h series for every sensor.
        var bundles = await historyClient.GetBundle(activeSensorIds, ct);

        return activeSensorIds.Select(id =>
        {
            bundles.TryGetValue(id, out var bundle);
            var deviceId = metadataCache.GetSensorDeviceId(id);
            var metadata = metadataCache.GetSensorMetadata(id);

            return new UiSensorResponse
            {
                Id = id,
                DeviceId = deviceId,
                Name = metadataCache.GetSensorName(id),
                DeviceName = metadataCache.GetDeviceName(deviceId),
                ShowName = UiCubeMapper.ShowName(metadata),
                Visible = UiCubeMapper.IsVisible(metadata),
                Latest = UiCubeMapper.ToUiValue(bundle?.Latest),
                Values = bundle is null ? [] : UiCubeMapper.ToPoints(bundle.Values),
                Thresholds = UiCubeMapper.Thresholds(metadata),
                Icons = UiCubeMapper.Icons(metadata),
            };
        }).ToList();
    }

    [HttpGet("actuators")]
    public async Task<IEnumerable<UiActuatorResponse>> GetActuators(CancellationToken ct)
    {
        var activeActuatorIds = (await historyClient.GetActiveActuatorIds(ct)).ToList();
        if (activeActuatorIds.Count == 0) return [];

        var sensorIds = activeActuatorIds
            .Select(id => metadataCache.GetActuatorSensorId(id))
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var latestValues = sensorIds.Count > 0
            ? await historyClient.GetLatestValuesBatch(sensorIds, ct)
            : new Dictionary<Guid, ApiEvent>();

        return activeActuatorIds.Select(id =>
        {
            var sensorId = metadataCache.GetActuatorSensorId(id);
            latestValues.TryGetValue(sensorId, out var latest);
            var deviceId = metadataCache.GetActuatorDeviceId(id);
            var metadata = metadataCache.GetActuatorMetadata(id);
            var (ioType, min, max, metaValue) = UiCubeMapper.IoType(metadata);

            return new UiActuatorResponse
            {
                Id = id,
                DeviceId = deviceId,
                SensorId = sensorId,
                Name = metadataCache.GetActuatorName(id),
                DeviceName = metadataCache.GetDeviceName(deviceId),
                ShowName = UiCubeMapper.ShowName(metadata),
                Visible = UiCubeMapper.IsVisible(metadata),
                Latest = UiCubeMapper.ToUiValue(latest),
                IoType = ioType,
                MinValue = min,
                MaxValue = max,
                MetadataValue = metaValue,
                Icons = UiCubeMapper.Icons(metadata),
            };
        }).ToList();
    }

    [HttpGet("sensor/{sensorId}/values")]
    public async Task<IEnumerable<ApiEvent>> GetSensorValues(Guid sensorId, CancellationToken ct)
    {
        return await historyClient.GetSensorValues(sensorId, ct);
    }
}
