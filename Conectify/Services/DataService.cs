namespace Conectify.Server.Services;

using AutoMapper;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Server.Caches;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Services.Data;
using Database.Models.Values;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

public interface IDataService
{
    Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default);
    Task ProcessEntity(IBaseInputType mapedEntity, Guid deviceId, CancellationToken ct = default);
    Task<IEnumerable<IApiDataModel>> ProcessData(IApiDataModel dataModel,  Guid deviceId, CancellationToken ct = default);
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
    private readonly IDataCache dataCache;

    public DataService(ILogger<DataService> logger, ConectifyDb database, IPipelineService pipelineService, IDeviceService deviceService, ISensorService sensorService, IActuatorService actuatorService, IMapper mapper, IDataCache dataCache)
    {
        this.logger = logger;
        this.database = database;
        this.pipelineService = pipelineService;
        this.deviceService = deviceService;
        this.sensorService = sensorService;
        this.actuatorService = actuatorService;
        this.mapper = mapper;
        this.dataCache = dataCache;
    }

    public async Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default)
    {
        try
        {
            var (mapedEntity, _) = SharedDataService.DeserializeJson(rawJson, mapper);

            await ProcessEntity(mapedEntity, deviceId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception catched when working with devices");
            logger.LogInformation(ex.Message);
            logger.LogDebug(ex.StackTrace);
            database.ChangeTracker.Clear();
        }
    }

    public async Task<IEnumerable<IApiDataModel>> ProcessData(IApiDataModel dataModel, Guid deviceId, CancellationToken ct = default)
    {
        var data = SharedDataService.ConvertToBase(dataModel, mapper);
        await ProcessEntity(data.Item1, deviceId, ct);
        return await GetDataForDevice(deviceId, ct);
    }


    public async Task ProcessEntity(IBaseInputType mapedEntity, Guid deviceId, CancellationToken ct = default)
    {
        if (await ValidateAndRepairEntity(mapedEntity, deviceId, ct))
        {
            await SaveToDatabase(mapedEntity);
            await pipelineService.ResendValueToSubscribers(mapedEntity);
        }
    }

    private async Task<IEnumerable<IApiDataModel>> GetDataForDevice(Guid deviceId, CancellationToken ct = default)
    {
        logger.LogInformation("Getting current data for device {deviceId}", deviceId);

        var lastCall = dataCache.GetLastCall(deviceId);

        var commands = await database.Commands.AsNoTracking().Where(x => x.DestinationId == deviceId && x.TimeCreated > lastCall).ToListAsync(ct);

        var actuators = (await actuatorService.GetAllActuatorsPerDevice(deviceId, ct)).Select(x => x.Id).ToList();
        var actions = await database.Actions.AsNoTracking().OrderByDescending(x => x.TimeCreated).Where(x => x.DestinationId.HasValue && actuators.Contains(x.DestinationId.Value)).ToListAsync(ct);

        var values = mapper.Map<IEnumerable<ApiDataModel>>(commands);
        values = values.Union(mapper.Map<IEnumerable<ApiDataModel>>(actions));

        dataCache.AddLastCall(deviceId);
        return values;
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
