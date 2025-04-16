namespace Conectify.Server.Services;

using Conectify.Database;
using Conectify.Shared.Library;
using Conectify.Shared.Services.Data;
using Database.Models.Values;

public interface IDataService
{
    Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default);
    Task ProcessEntity(Event mapedEntity, Guid deviceId, CancellationToken ct = default);
}

public class DataService(ILogger<DataService> logger, ConectifyDb database, IPipelineService pipelineService, IDeviceService deviceService, ISensorService sensorService) : IDataService
{
    public async Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default)
    {
        try
        {
            var e = SharedDataService.DeserializeJson(rawJson);

            if (e == null)
            {
                logger.LogError("Could not deserialize incoming event {message}", rawJson);
                return;
            }

            await ProcessEntity(e, deviceId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception catched when working with devices");
            logger.LogInformation("{message}", ex.Message);
            logger.LogDebug("{stack}", ex.StackTrace);
            database.ChangeTracker.Clear();
        }
    }

    public async Task ProcessEntity(Event e, Guid deviceId, CancellationToken ct = default)
    {
        try
        {
            if (await ValidateAndRepairEvent(e, deviceId, ct))
            {
                await Tracing.Trace(async () =>
                {
                    await SaveToDatabase(e);
                    await pipelineService.ResendEventToSubscribers(e);
                }, e.SourceId, "Processing Event");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("{Message}", ex.Message);
        }
    }

    private async Task<bool> ValidateAndRepairEvent(Event? evnt, Guid deviceId, CancellationToken ct = default)
    {
        if (evnt == null)
        {
            return false;
        }

        if (evnt.Id == Guid.Empty)
        {
            evnt.Id = Guid.NewGuid();
        }
        await Tracing.Trace(async () =>
        {
            // repairs of unknown references
            if (evnt.Type is Constants.Events.Value)
            {
                await sensorService.TryAddUnknownDevice(evnt.SourceId, deviceId, ct);
            }

            if (evnt.Type is Constants.Events.Command)
            {
                await deviceService.TryAddUnknownDevice(evnt.SourceId, evnt.SourceId, ct);
            }
        }, evnt.Id, "Processing and validating Event");
        return true;
    }

    private async Task SaveToDatabase(Event mapedEntity)
    {
        await Tracing.Trace(async () =>
        {
            await database.Events.AddAsync(mapedEntity);
            await database.SaveChangesAsync();
            logger.LogInformation("Saved to database");
        }, mapedEntity.Id, "Saving event to DB");
    }
}
