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

            IEnumerable<Guid> targetingSubscribers = GetTargetsForEvent(evnt);

            foreach (var subscriber in targetingSubscribers.Distinct())
            {
                logger.LogInformation("Sending from pipeline to {subscriber}", subscriber);
                await webSocketService.SendToDeviceAsync(subscriber, apiModel);
            }
        }, evnt.Id, "Resending to subscribers");
        sw.Stop();
        var meter = meterFactory.Create("PipelineService_ResendEventToSubscribers_Duration");
        var histogram = meter.CreateHistogram<double>("PipelineService_ResendEventToSubscribers_Duration", "ms", "Duration of ResendEventToSubscribers in milliseconds");
        histogram.Record(sw.Elapsed.TotalMilliseconds);
    }

    private IEnumerable<Guid> GetTargetsForEvent(Event evnt)
    {
        var subs = GetAllSubscribers().Where(x => x.IsSubedToAll || x.Preferences.Any(x => x.EventType == Constants.Events.All || (x.EventType == evnt.Type && (x.SubscibeeId is null || x.SubscibeeId == evnt.SourceId)))).Select(x => x.DeviceId);

        if (evnt.DestinationId.HasValue)
        {
            var target = GetAllSubscribers().FirstOrDefault(x => x.AllDependantIds.Contains(evnt.DestinationId.Value));

            if (target is not null)
            {
                subs = subs.Append(target.DeviceId);
            }
        }

        return subs;

    }

}
