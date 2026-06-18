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

        var latestValues = await historyClient.GetLatestValuesBatch(activeSensorIds, ct);

        return activeSensorIds.Select(id =>
        {
            latestValues.TryGetValue(id, out var latest);
            var deviceId = metadataCache.GetSensorDeviceId(id);
            return new UiSensorResponse
            {
                Id = id,
                Name = metadataCache.GetSensorName(id),
                SourceDeviceId = deviceId,
                DeviceName = metadataCache.GetDeviceName(deviceId),
                Metadata = metadataCache.GetSensorMetadata(id),
                LatestValue = latest,
            };
        });
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
            return new UiActuatorResponse
            {
                Id = id,
                Name = metadataCache.GetActuatorName(id),
                SourceDeviceId = deviceId,
                SensorId = sensorId,
                DeviceName = metadataCache.GetDeviceName(deviceId),
                Metadata = metadataCache.GetActuatorMetadata(id),
                LatestValue = latest,
            };
        });
    }

    [HttpGet("sensor/{sensorId}/values")]
    public async Task<IEnumerable<ApiEvent>> GetSensorValues(Guid sensorId, CancellationToken ct)
    {
        return await historyClient.GetSensorValues(sensorId, ct);
    }
}
