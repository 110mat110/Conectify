namespace Conectify.Server.Caches;

using AutoMapper;
using Conectify.Database;
using Microsoft.EntityFrameworkCore;

public interface ISubscribersCache
{
    Subscriber? GetSubscriber(Guid deviceId);

    bool RemoveSubscriber(Guid deviceId);
    Task<bool> AddSubscriber(Guid deviceId, CancellationToken ct = default);

    IEnumerable<Subscriber> AllSubscribers();
}

public class SubscribersCache : ISubscribersCache
{
    private static Dictionary<Guid, Subscriber> subscribers = new Dictionary<Guid, Subscriber>();
    private readonly IServiceProvider serviceProvider;
    private readonly IMapper mapper;

    public SubscribersCache(IServiceProvider serviceProvider, IMapper mapper)
    {
        this.serviceProvider = serviceProvider;
        this.mapper = mapper;
    }

    public async Task<bool> AddSubscriber(Guid deviceId, CancellationToken ct = default)
    {
        if (!subscribers.ContainsKey(deviceId))
        {
            var subscriber = await UpdateSubscriber(deviceId, ct);
            if (subscriber is not null)
            {
                subscribers.Add(deviceId, subscriber);
                return true;
            }
        }

        return false;
    }

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
        return subscribers.Remove(deviceId);
    }

    public async Task<Subscriber?> UpdateSubscriber(Guid deviceId, CancellationToken ct = default)
    {
        await using var db = serviceProvider.GetRequiredService<ConectifyDb>();

        var device = await db.Devices.AsNoTracking().Include(x => x.Preferences).Include(x => x.Sensors).Include(x => x.Actuators).FirstOrDefaultAsync(x => x.Id == deviceId, ct);

        if (device is not null)
        {
            return mapper.Map<Subscriber>(device);
        }

        return null;
    }
}