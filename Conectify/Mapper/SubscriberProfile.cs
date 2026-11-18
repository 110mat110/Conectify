namespace Conectify.Server.Mapper;

using Conectify.Database.Models;
using Conectify.Server.Caches;
using Conectify.Shared.Library.Mapping;

public class SubscriberProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        configuration.CreateMap<Device, Subscriber>(x => new Subscriber
        {
            DeviceId = x.Id,
            IsSubedToAll = x.SubscribeToAll,
            AllDependantIds = (x.Sensors ?? new List<Sensor>()).Select(s => s.Id)
                .Concat((x.Actuators ?? new List<Actuator>()).Select(a => a.Id))
                .Concat(new List<Guid>() { x.Id })
                .ToList(),
            Preferences = (x.Preferences ?? new List<Preference>()).ToList(),
        });
    }
}
