namespace Conectify.Server.Services;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Shared.Library.ErrorHandling;
using Database.Models.Values;
using Newtonsoft.Json;
using System.Data.Entity;
using System.Reflection;

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
    private const string ApiPrefix = "Api";

    public DataService(ILogger<DataService> logger, ConectifyDb database, IPipelineService pipelineService, IDeviceService deviceService, ISensorService sensorService, IActuatorService actuatorService)
    {
        this.logger = logger;
        this.database = database;
        this.pipelineService = pipelineService;
        this.deviceService = deviceService;
        this.sensorService = sensorService;
        this.actuatorService = actuatorService;
    }

    public async Task InsertJsonModel(string rawJson, Guid deviceId, CancellationToken ct = default)
    {
        try
        {
            var mapedEntity = DeserializeJson(rawJson);

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
        }
    }

    private async Task<bool> ValidateAndRepairEntity(IBaseInputType mapedEntity, Guid deviceId, CancellationToken ct = default)
    {
        //TODO validations will have place here

        // repairs of unknown references
        if(mapedEntity is Value or Action)
        {
             await sensorService.TryAddUnknownDevice(mapedEntity.SourceId, deviceId);
        }

        if (mapedEntity is Command or CommandResponse)
        {
            await deviceService.TryAddUnknownDevice(mapedEntity.SourceId, mapedEntity.SourceId);
        }

        if (mapedEntity is ActionResponse)
        {
            await actuatorService.TryAddUnknownDevice(mapedEntity.SourceId, deviceId);
        }

        return true;
    }

    private async Task SaveToDatabase(IBaseInputType mapedEntity)
    {
        await database.AddAsync(mapedEntity);
        await database.SaveChangesAsync();
    }

    private IBaseInputType DeserializeJson(string rawJson)
    {
        var type = JsonConvert.DeserializeAnonymousType(rawJson, new { Type = string.Empty });
        var expectedTypeName = (type != null ? type.Type : string.Empty);
        Assembly asm = typeof(Value).Assembly;
        Type? t = asm.GetTypes().FirstOrDefault(x => x.Name.ToLower() == expectedTypeName.ToLower());

        if (t is null)
        {
            throw new ConectifyException($"Does not recognize type {(type != null ? type.Type : string.Empty)}");
        }

        var deserialized = JsonConvert.DeserializeObject(rawJson, t) as IBaseInputType;

        if(deserialized is null)
        {
            throw new ConectifyException($"Could not serialize {rawJson} to type {t.Name}");
        }

        return deserialized;
    }
}
