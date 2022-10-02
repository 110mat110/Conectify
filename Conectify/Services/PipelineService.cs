namespace Conectify.Server.Services;

using AutoMapper;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Conectify.Server.Caches;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

public interface IPipelineService
{
    Task ResendValueToSubscribers(IBaseInputType entity);

    Task SetPreference(Guid deviceId, IEnumerable<ApiPreference> apiPreferences, CancellationToken ct = default);

    Task SetSubscribeToAll(Guid deviceId, bool sub, CancellationToken ct = default);

}

public class PipelineService : IPipelineService
{
    private readonly ConectifyDb conectifyDb;
    private readonly ISubscribersCache subscribersCache;
    private readonly IWebSocketService webSocketService;
    private readonly IMapper mapper;
    private readonly ILogger<PipelineService> logger;

    public PipelineService(ConectifyDb conectifyDb, ISubscribersCache subscribersCache, IWebSocketService webSocketService, IMapper mapper, ILogger<PipelineService> logger)
    {
        this.conectifyDb = conectifyDb;
        this.subscribersCache = subscribersCache;
        this.webSocketService = webSocketService;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task ResendValueToSubscribers(IBaseInputType entity)
    {
        IWebsocketModel? apiModel = null;
        IEnumerable<Guid> targetingSubscribers = new List<Guid>();
        if (entity is Value v)
        {
            apiModel = mapper.Map<WebsocketValue>(entity);
            targetingSubscribers = ValueTargetingSubscribers(v.SourceId);
        }

        if (entity is Command command)
        {
            apiModel = mapper.Map<WebsocketCommand>(entity);
            targetingSubscribers = targetingSubscribers.Concat(CommandTargetingSubscribers(command.DestinationId, command.SourceId));
        }

        if (entity is Action action)
        {
            apiModel = mapper.Map<WebsocketAction>(entity);
            targetingSubscribers = ActionTargetingSubscribers(action.SourceId, action.DestinationId);
        }

        if (entity is CommandResponse cr)
        {
            var commandSourceId = conectifyDb.Commands.FirstOrDefault(x => x.Id == cr.CommandId)?.SourceId;

            apiModel = mapper.Map<WebsocketCommandResponse>(entity);
            targetingSubscribers = CommandResponseTargetingSubscribers(cr.SourceId, commandSourceId);
        }

        if (entity is ActionResponse ar)
        {
            var actionSourceId = conectifyDb.Actions.FirstOrDefault(x => x.Id == ar.ActionId)?.SourceId;


            apiModel = mapper.Map<WebsocketActionResponse>(entity);
            targetingSubscribers = ActionResponseTargetingSubscribers(ar.SourceId, actionSourceId);
        }

        if (apiModel is null)
        {
            return;
        }

        foreach (var sub in subscribersCache.AllSubscribers()){
            logger.LogTrace(sub.DeviceId.ToString() + sub.IsSubedToAll);
        }
        foreach (var subscriber in targetingSubscribers.Distinct())
        {
            this.logger.LogWarning("Sending from pipeline to " + subscriber.ToString());
            await webSocketService.SendToDeviceAsync(subscriber, apiModel);
        }
    }

    public async Task SetPreference(Guid deviceId, IEnumerable<ApiPreference> apiPreferences, CancellationToken ct = default)
    {
        var device = await conectifyDb
            .Set<Device>()
            .Include(i => i.Preferences)
            .FirstOrDefaultAsync(x => x.Id == deviceId && x.IsKnown, ct);

        if (device is null) return;

        var preferences = mapper.Map<IEnumerable<Preference>>(apiPreferences);
        foreach(var preference in preferences)
        {
            preference.SubscriberId = deviceId;
        };

        var preferencesToRemove = device.Preferences
            .Where(x => preferences.Any(p =>
                p.ActuatorId == x.ActuatorId &&
                p.SensorId == x.SensorId &&
                p.DeviceId == x.DeviceId
                ));
        device.Preferences = device.Preferences.Except(preferencesToRemove).ToHashSet();
        device.Preferences.Concat(preferences);
        device.SubscribeToAll = device.Preferences.Any(IsSubbedToAll);
        await conectifyDb.AddRangeAsync(preferences);
        conectifyDb.Update(device);
        await conectifyDb.SaveChangesAsync(ct);

        await subscribersCache.UpdateSubscriber(deviceId, ct);
    }

    public async Task SetSubscribeToAll(Guid deviceId, bool sub, CancellationToken ct = default)
    {
        var device = await conectifyDb.Set<Device>().FirstOrDefaultAsync(x => x.Id == deviceId && x.IsKnown, cancellationToken: ct);

        if (device is null) return;

        device.SubscribeToAll = sub;

        await conectifyDb.SaveChangesAsync(ct);
        await subscribersCache.UpdateSubscriber(deviceId, ct);
    }

    public IEnumerable<Subscriber> GetAllSubscribers() => subscribersCache.AllSubscribers();

    private IEnumerable<Guid> ActionResponseTargetingSubscribers(Guid sourceId, Guid? actionSourceId) =>
    GetAllSubscribers()
    .Where(x =>
        x is not null &&(
        x.IsSubedToAll ||
        (actionSourceId is not null && x.Sensors.Contains(actionSourceId.Value)) ||
        x.Preferences.Any(preference =>
            preference.SubToCommandResponse &&
                (preference.SensorId is null ||
                preference.SensorId == sourceId))))
    .Select(s => s.DeviceId);

    private IEnumerable<Guid> CommandResponseTargetingSubscribers(Guid sourceId, Guid? commandSourceId) =>
        GetAllSubscribers()
        .Where(x =>
            x is not null && (
            x.IsSubedToAll ||
            x.DeviceId == commandSourceId ||
            x.Preferences.Any(preference =>
                preference.SubToCommandResponse &&
                    (preference.DeviceId is null ||
                    preference.DeviceId == sourceId))))
        .Select(s => s.DeviceId);

    private IEnumerable<Guid> ValueTargetingSubscribers(Guid sourceId) =>
        GetAllSubscribers()
        .Where(x =>
            x is not null && (
            x.IsSubedToAll ||
            x.Preferences.Any(preference =>
                preference.SubToValues &&
                    (preference.SensorId is null ||
                    preference.SensorId == sourceId))))
        .Select(s => s.DeviceId);

    private IEnumerable<Guid> CommandTargetingSubscribers(Guid targetId, Guid sourceId) =>
         GetAllSubscribers()
        .Where(x => x is not null && (x.IsSubedToAll || x.DeviceId == targetId || x.Preferences.Any(preference =>
                preference.SubToCommands &&
                    (preference.SensorId is null ||
                    preference.SensorId == sourceId))))
        .Select(x => x.DeviceId);

    private IEnumerable<Guid> ActionTargetingSubscribers(Guid sourceId, Guid? destinationId) =>
        GetAllSubscribers()
        .Where(x => x is not null && (x.IsSubedToAll || (destinationId != null && x.Actuators.Contains(destinationId.Value)) || x.Preferences.Any(preference =>
                preference.SubToActions &&
                    (preference.SensorId is null ||
                    preference.SensorId == sourceId))))
        .Select(x => x.DeviceId);

    private readonly Func<Preference, bool> IsSubbedToAll = x =>
        x.SensorId is null &&
        x.DeviceId is null &&
        x.SensorId is null &&
        x.SubToValues &&
        x.SubToCommands &&
        x.SubToActions &&
        x.SubToActionResponse &&
        x.SubToCommandResponse;

}
