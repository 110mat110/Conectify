namespace Conectify.Server.Services;

using System.Diagnostics.Metrics;
using System.Linq;
using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Conectify.Server.Caches;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public interface IPipelineService
{
    Task ResendEventToSubscribers(Event e);

    Task SetPreference(Guid deviceId, IEnumerable<ApiPreference> apiPreferences, CancellationToken ct = default);

    Task SetSubscribeToAll(Guid deviceId, bool sub, CancellationToken ct = default);

}

public class PipelineService(ConectifyDb conectifyDb, ISubscribersCache subscribersCache, IWebSocketService webSocketService, IMapper mapper, ILogger<PipelineService> logger, IMeterFactory meterFactory) : IPipelineService
{
    public async Task SetPreference(Guid deviceId, IEnumerable<ApiPreference> apiPreferences, CancellationToken ct = default)
    {
        var device = await conectifyDb
            .Set<Device>()
            .Include(i => i.Preferences)
            .FirstOrDefaultAsync(x => x.Id == deviceId && x.IsKnown, ct);

        if (device is null) return;

        var preferences = mapper.Map<List<Preference>>(apiPreferences);
        foreach (var preference in preferences)
        {
            preference.SubscriberId = deviceId;
        }
        ;

        var filteredPreferences = preferences
            .Where(newPref => !device.Preferences.Any(existingPref =>
                existingPref.SubscibeeId == newPref.SubscibeeId &&
                existingPref.EventType == newPref.EventType))
            .ToList();


        device.Preferences.Concat(filteredPreferences);
        await conectifyDb.AddRangeAsync(filteredPreferences, ct);
        device.SubscribeToAll = device.Preferences.Any(x => x.EventType == Constants.Events.All);
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

    public async Task ResendEventToSubscribers(Event evnt)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Tracing.Trace(async () =>
        {
            var apiModel = mapper.Map<WebsocketEvent>(evnt);

            if (apiModel is null)
            {
                return;
            }

            IEnumerable<Guid> targetingSubscribers = await GetTargetsForEvent(evnt);

            foreach (var subscriber in targetingSubscribers.Distinct())
            {
                logger.LogInformation("Sending from pipeline to {subscriber}", subscriber);
                await webSocketService.SendToDeviceAsync(subscriber, apiModel);
            }
        }, evnt.Id, "Resending to subscribers");
        sw.Stop();
        var meter = meterFactory.Create("CustomMeters");
        var histogram = meter.CreateHistogram<double>("PipelineService_ResendEventToSubscribers_Duration", "ms", "Duration of ResendEventToSubscribers in milliseconds");
        histogram.Record(sw.Elapsed.TotalMilliseconds);
    }

    private async Task<IEnumerable<Guid>> GetTargetsForEvent(Event evnt)
    {
        var allSubs = GetAllSubscribers().ToList();
        var subs = allSubs
            .Where(x => x.IsSubedToAll || x.Preferences.Any(x => x.EventType == Constants.Events.All || (x.EventType == evnt.Type && (x.SubscibeeId is null || x.SubscibeeId == evnt.SourceId))))
            .Select(x => x.DeviceId)
            .ToList();

        logger.LogDebug("GetTargets: eventId={EventId} type={Type} sourceId={SourceId} broadcastSubscribers={Count} totalCachedSubscribers={Total}",
            evnt.Id, evnt.Type, evnt.SourceId, subs.Count, allSubs.Count);

        if (evnt.DestinationId.HasValue)
        {
            var target = allSubs.FirstOrDefault(x => x.AllDependantIds.Contains(evnt.DestinationId.Value));

            if (target is not null)
            {
                logger.LogInformation("GetTargets: eventId={EventId} destinationId={DestinationId} found in subscriber cache — routing to deviceId={DeviceId}",
                    evnt.Id, evnt.DestinationId.Value, target.DeviceId);
            }
            else
            {
                logger.LogInformation("GetTargets: eventId={EventId} destinationId={DestinationId} not in subscriber cache — looking up in DB",
                    evnt.Id, evnt.DestinationId.Value);

                var sourceDeviceId = await FindSourceDeviceId(evnt.DestinationId.Value);

                if (sourceDeviceId.HasValue)
                {
                    logger.LogInformation("GetTargets: eventId={EventId} destinationId={DestinationId} resolved to sourceDeviceId={SourceDeviceId} — updating subscriber cache",
                        evnt.Id, evnt.DestinationId.Value, sourceDeviceId.Value);
                    target = await subscribersCache.UpdateSubscriber(sourceDeviceId.Value);

                    if (target is null)
                    {
                        logger.LogWarning("GetTargets: eventId={EventId} destinationId={DestinationId} sourceDeviceId={SourceDeviceId} — UpdateSubscriber returned null (device not in DB?)",
                            evnt.Id, evnt.DestinationId.Value, sourceDeviceId.Value);
                    }
                }
                else
                {
                    logger.LogWarning("GetTargets: eventId={EventId} destinationId={DestinationId} — could not find owning device in DB (actuator/sensor/device not registered?)",
                        evnt.Id, evnt.DestinationId.Value);
                }
            }

            if (target is not null)
            {
                subs.Add(target.DeviceId);
            }
            else
            {
                logger.LogWarning("GetTargets: eventId={EventId} destinationId={DestinationId} — no target device found, message will NOT be delivered",
                    evnt.Id, evnt.DestinationId.Value);
            }
        }

        logger.LogInformation("GetTargets: eventId={EventId} final recipients={Recipients}",
            evnt.Id, string.Join(", ", subs.Distinct()));

        return subs;
    }

    private async Task<Guid?> FindSourceDeviceId(Guid destinationId)
    {
        var actuator = await conectifyDb.Set<Actuator>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == destinationId);
        if (actuator is not null)
        {
            logger.LogDebug("FindSourceDeviceId: {DestinationId} is actuator — sourceDeviceId={SourceDeviceId}", destinationId, actuator.SourceDeviceId);
            return actuator.SourceDeviceId;
        }

        var sensor = await conectifyDb.Set<Sensor>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == destinationId);
        if (sensor is not null)
        {
            logger.LogDebug("FindSourceDeviceId: {DestinationId} is sensor — sourceDeviceId={SourceDeviceId}", destinationId, sensor.SourceDeviceId);
            return sensor.SourceDeviceId;
        }

        var device = await conectifyDb.Set<Device>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == destinationId);
        if (device is not null)
        {
            logger.LogDebug("FindSourceDeviceId: {DestinationId} is device", destinationId);
            return device.Id;
        }

        logger.LogWarning("FindSourceDeviceId: {DestinationId} not found as actuator, sensor, or device", destinationId);
        return null;
    }

}
