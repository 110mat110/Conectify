namespace Conectify.Server.Mapper;

using AutoMapper;
using Conectify.Database.Models;
using Conectify.Server.Caches;

public class SubscriberProfile : Profile
{
    public SubscriberProfile()
    {
        this.CreateMap<Device, Subscriber>()
            .ForMember(x => x.IsSubedToAll, opt => opt.MapFrom(x => x.SubscribeToAll))
            .ForMember(x => x.Actuators, opt => opt.MapFrom(x => x.Actuators.Select(x => x.Id)))
            .ForMember(x => x.Sensors, opt => opt.MapFrom(x => x.Sensors.Select(x => x.Id)))
            .ForMember(x => x.DeviceId, opt => opt.MapFrom(x => x.Id))
            .ForMember(x => x.Preferences, opt => opt.MapFrom(x => x.Preferences));
    }
}
