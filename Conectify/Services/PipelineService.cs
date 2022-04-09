namespace Conectify.Server.Services;

using AutoMapper;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Database.Models.Values;
using Conectify.Server.Caches;
using Conectify.Shared.Library.Models.Values;
using System.Linq;

public interface IPipelineService
{
    Task ResendValueToSubscribers(IBaseInputType entity);
}

public class PipelineService : IPipelineService
{
    private readonly ConectifyDb conectifyDb;
    private readonly ISubscribersCache subscribersCache;
    private readonly IWebSocketService webSocketService;
    private readonly IMapper mapper;

    public PipelineService(ConectifyDb conectifyDb, ISubscribersCache subscribersCache, IWebSocketService webSocketService, IMapper mapper)
    {
        this.conectifyDb = conectifyDb;
        this.subscribersCache = subscribersCache;
        this.webSocketService = webSocketService;
        this.mapper = mapper;
    }

    public async Task ResendValueToSubscribers(IBaseInputType entity)
    {
        IApiBaseModel? apiModel = null;
        IEnumerable<Guid> targetingSubscribers = new List<Guid>();
        if (entity is Value v)
        {
            apiModel = mapper.Map<ApiValue>(entity);
            targetingSubscribers = ValueTargetingSubscribers(v.SourceId);
        }

        if (entity is Command command)
        {
            apiModel = mapper.Map<ApiCommand>(entity);
            targetingSubscribers = targetingSubscribers.Concat(CommandTargetingSubscribers(command.DestinationId, command.SourceId));
        }

        if (entity is Action action)
        {
            apiModel = mapper.Map<ApiAction>(entity);
            targetingSubscribers = ActionTargetingSubscribers(action.SourceId, action.DestinationId);
        }

        if (entity is CommandResponse cr)
        {
            var commandSourceId = conectifyDb.Commands.FirstOrDefault(x => x.Id == cr.CommandId)?.SourceId;
           
            apiModel = mapper.Map<ApiCommandResponse>(entity);
            targetingSubscribers = CommandResponseTargetingSubscribers(cr.SourceId, commandSourceId);
        }

        if (entity is ActionResponse ar)
        {
            var actionSourceId = conectifyDb.Actions.FirstOrDefault(x => x.Id == ar.ActionId)?.SourceId;


            apiModel = mapper.Map<ApiActionResponse>(entity);
            targetingSubscribers = ActionResponseTargetingSubscribers(ar.SourceId, actionSourceId);
        }

        if (apiModel is null)
        {
            return;
        }

        foreach (var subscriber in targetingSubscribers.Distinct())
        {
            await webSocketService.SendToThingAsync(subscriber, apiModel);
        }
    }

    public IEnumerable<Subscriber> GetAllSubscribers() => subscribersCache.AllSubscribers();

    private IEnumerable<Guid> ActionResponseTargetingSubscribers(Guid sourceId, Guid? actionSourceId) =>
    GetAllSubscribers()
    .Where(x =>
        x.IsSubedToAll ||
        (actionSourceId is not null && x.Sensors.Contains(actionSourceId.Value)) ||
        x.Preferences.Any(preference =>
            preference.SubToCommandResponse &&
                (preference.SensorId is null ||
                preference.SensorId == sourceId)))
    .Select(s => s.DeviceId);

    private IEnumerable<Guid> CommandResponseTargetingSubscribers(Guid sourceId, Guid? commandSourceId) =>
        GetAllSubscribers()
        .Where(x =>
            x.IsSubedToAll ||
            x.DeviceId == commandSourceId ||
            x.Preferences.Any(preference =>
                preference.SubToCommandResponse &&
                    (preference.DeviceId is null ||
                    preference.DeviceId == sourceId)))
        .Select(s => s.DeviceId);

    public IEnumerable<Guid> ValueTargetingSubscribers(Guid sourceId) =>
        GetAllSubscribers()
        .Where(x =>
            x.IsSubedToAll ||
            x.Preferences.Any(preference =>
                preference.SubToValues &&
                    (preference.SensorId is null ||
                    preference.SensorId == sourceId)))
        .Select(s => s.DeviceId);

    public IEnumerable<Guid> CommandTargetingSubscribers(Guid targetId, Guid sourceId) =>
         GetAllSubscribers()
        .Where(x => x.IsSubedToAll || x.DeviceId == targetId || x.Preferences.Any(preference =>
                preference.SubToCommands &&
                    (preference.SensorId is null ||
                    preference.SensorId == sourceId)))
        .Select(x => x.DeviceId);

    public IEnumerable<Guid> ActionTargetingSubscribers(Guid sourceId, Guid? destinationId) =>
        GetAllSubscribers()
        .Where(x => x.IsSubedToAll || (destinationId != null && x.Actuators.Contains(destinationId.Value)) || x.Preferences.Any(preference =>
                preference.SubToActions &&
                    (preference.SensorId is null ||
                    preference.SensorId == sourceId)))
        .Select(x => x.DeviceId);
}
