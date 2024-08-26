namespace Conectify.Server.Caches;

using AutoMapper;
using Conectify.Database;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

public interface ISubscribersCache
{
    Subscriber? GetSubscriber(Guid deviceId);

    bool RemoveSubscriber(Guid deviceId);

    IEnumerable<Subscriber> AllSubscribers();

    Task<Subscriber?> UpdateSubscriber(Guid deviceId, CancellationToken ct = default);
}

public class SubscribersCache(IServiceProvider serviceProvider, IMapper mapper) : ISubscribersCache
{
    private static Dictionary<Guid, Subscriber> subscribers = new Dictionary<Guid, Subscriber>();
    private readonly object locker = new();

    public IEnumerable<Subscriber> AllSubscribers() => subscribers.Select(x => x.Value);

    public Subscriber? GetSubscriber(Guid deviceId)
    {
        if (subscribers.ContainsKey(deviceId))
        {
            return subscribers[deviceId];
        }
        return null;
    }

    public bool RemoveSubscriber(Guid deviceId)
    {
        
        var result = subscribers.Remove(deviceId);

        if (result)
        {
            using var scope = serviceProvider.CreateScope();
            var meterFactory = scope.ServiceProvider.GetService<IMeterFactory>();
            if (meterFactory is not null)
            {
                var meter = meterFactory.Create("CustomMeters");
                var counter = meter.CreateCounter<int>("subs_count");
                counter.Add(-1);
            }
            return true;
        }
        return false;
    }

    public async Task<Subscriber?> UpdateSubscriber(Guid deviceId, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<ConectifyDb>();

        var device = await db.Devices.AsNoTracking().Include(x => x.Preferences).Include(x => x.Sensors).Include(x => x.Actuators).FirstOrDefaultAsync(x => x.Id == deviceId, ct);

        if (device is not null)
        {
            var sub = mapper.Map<Subscriber>(device);
            if (subscribers.ContainsKey(deviceId))
            {
                lock (locker) { subscribers[deviceId] = sub; }
            }
            else
            {
                lock (locker) { subscribers.Add(deviceId, sub); }
                var meterFactory = scope.ServiceProvider.GetService<IMeterFactory>();
                if (meterFactory is not null)
                {
                    var meter = meterFactory.Create("CustomMeters");
                    var counter = meter.CreateCounter<int>("subs_count");
                    counter.Add(1);
                }
            }
            return subscribers[deviceId];
        }

        return null;
    }
}