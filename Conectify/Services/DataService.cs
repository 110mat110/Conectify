namespace Conectify.Server.Services;

using AutoMapper;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Shared.Services.Data;
using Database.Models.Values;

public interface IDataService
{
    Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default);
}

public class DataService : IDataService
{
    private readonly ILogger<DataService> logger;
    private readonly ConectifyDb database;
    private readonly IPipelineService pipelineService;
    private readonly IDeviceService deviceService;
    private readonly ISensorService sensorService;
    private readonly IActuatorService actuatorService;
    private readonly IMapper mapper;

    public DataService(ILogger<DataService> logger, ConectifyDb database, IPipelineService pipelineService, IDeviceService deviceService, ISensorService sensorService, IActuatorService actuatorService, IMapper mapper)
    {
        this.logger = logger;
        this.database = database;
        this.pipelineService = pipelineService;
        this.deviceService = deviceService;
        this.sensorService = sensorService;
        this.actuatorService = actuatorService;
        this.mapper = mapper;
    }

    public async Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default)
    {
        try
        {
            var (mapedEntity, _) = SharedDataService.DeserializeJson(rawJson, mapper);

            if (await ValidateAndRepairEntity(mapedEntity, deviceId, ct))
            {
                await SaveToDatabase(mapedEntity);
                await pipelineService.ResendValueToSubscribers(mapedEntity);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception catched when working with devices");
            logger.LogInformation(ex.Message);
            logger.LogDebug(ex.StackTrace);
            database.ChangeTracker.Clear();
        }
    }

    private async Task<bool> ValidateAndRepairEntity(IBaseInputType mapedEntity, Guid deviceId, CancellationToken ct = default)
    {
        //TODO validations will have place here

        // repairs of unknown references
        if (mapedEntity is Value or Action)
        {
            await sensorService.TryAddUnknownDevice(mapedEntity.SourceId, deviceId, ct);
        }

        if (mapedEntity is Command or CommandResponse)
        {
            await deviceService.TryAddUnknownDevice(mapedEntity.SourceId, mapedEntity.SourceId, ct);
        }

        if (mapedEntity is ActionResponse)
        {
            await actuatorService.TryAddUnknownDevice(mapedEntity.SourceId, deviceId, ct);
        }

        return true;
    }

    private async Task SaveToDatabase(IBaseInputType mapedEntity)
    {

        var existingEntity = await database.FindAsync(mapedEntity.GetType(), mapedEntity.Id);
        if (existingEntity is null)
        {
            await database.AddAsync(mapedEntity);
            await database.SaveChangesAsync();
        }
    }
}
