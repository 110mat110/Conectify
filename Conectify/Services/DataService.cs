namespace Conectify.Server.Services;

using AutoMapper;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Shared.Library.Models;
using Database.Models.Values;

public interface IDataService
{
    Task InsertApiValue(ApiValueModel apiValue);
}

public class DataService : IDataService
{
    private readonly ILogger<DataService> logger;
    private readonly IMapper mapper;
    private readonly ConectifyDb database;

    public DataService(ILogger<DataService> logger, IMapper mapper, ConectifyDb database)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.database = database;
    }

    public async Task InsertApiValue(ApiValueModel apiValue)
    {
        logger.LogInformation($"Got value: {apiValue.ToJson()}");
        try
        {
            IBaseInputType mapedEntity = MapEntity(apiValue);
            await SaveToDatabase(mapedEntity);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception catched at input");
            logger.LogInformation(ex.Message);
            logger.LogDebug(ex.StackTrace);
        }
    }

    private async Task SaveToDatabase(IBaseInputType mapedEntity)
    {
        await database.AddAsync(mapedEntity);
        await database.SaveChangesAsync();
    }

    private IBaseInputType MapEntity(ApiValueModel apiValue)
    {
        switch (apiValue.Type)
        {
            case nameof(Command): return mapper.Map<Command>(apiValue);
            case nameof(Action): return mapper.Map<Action>(apiValue);
            case nameof(Value): return mapper.Map<Value>(apiValue);
            case nameof(ActionResponse): return mapper.Map<ActionResponse>(apiValue);
            case nameof(CommandResponse): return mapper.Map<CommandResponse>(apiValue);

            default: return null;
        }

    }
}
