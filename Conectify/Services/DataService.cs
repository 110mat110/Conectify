namespace Conectify.Server.Services;

using AutoMapper;
using Conectify.Database;
using Conectify.Shared.Library;
using Conectify.Shared.Services.Data;
using Database.Models.Values;

public interface IDataService
{
    Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default);
    Task ProcessEntity(Event mapedEntity, Guid deviceId, CancellationToken ct = default);
}

public class DataService(ILogger<DataService> logger, ConectifyDb database, IPipelineService pipelineService, IDeviceService deviceService, ISensorService sensorService, IActuatorService actuatorService, IMapper mapper) : IDataService
{
    public async Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default)
    {
        try
        {
            var e = SharedDataService.DeserializeJson(rawJson, mapper);

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
            logger.LogInformation(ex.Message);
            logger.LogDebug(ex.StackTrace);
            database.ChangeTracker.Clear();
        }
    }

    public async Task ProcessEntity(Event e, Guid deviceId, CancellationToken ct = default)
    {
        try
        {
            if (await ValidateAndRepairEvent(e, deviceId, ct))
            {
                await pipelineService.ResendEventToSubscribers(e);
                await SaveToDatabase(e);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex.Message}", ex);
        }
    }

    private async Task<bool> ValidateAndRepairEvent(Event? evnt, Guid deviceId, CancellationToken ct = default)
    {
        //TODO validations will have place here

        if (evnt == null)
        {
            return false;
        }

        // repairs of unknown references
        if (evnt.Type is Constants.Events.Value or Constants.Events.Value)
        {
            await sensorService.TryAddUnknownDevice(evnt.SourceId, deviceId, ct);
        }

        if (evnt.Type is Constants.Events.Command)
        {
            await deviceService.TryAddUnknownDevice(evnt.SourceId, evnt.SourceId, ct);
        }

        return true;
    }

    private async Task SaveToDatabase(Event mapedEntity)
    {
            await database.Events.AddAsync(mapedEntity);
            await database.SaveChangesAsync();
    }
}
