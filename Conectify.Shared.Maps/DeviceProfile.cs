namespace Conectify.Shared.Maps;

using AutoMapper;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;

public class DeviceProfile : Profile
{
    public DeviceProfile()
    {
        this.CreateMap<ApiDevice, Device>()
            .ForMember(x => x.IsKnown, opt => opt.MapFrom(x => false))
            .ForMember(x => x.SubscribeToAll, opt => opt.MapFrom(x => false))
            .ForMember(x => x.Metadata, opt => opt.Ignore())
            .ForMember(x => x.Sensors, opt => opt.Ignore())
            .ForMember(x => x.Actuators, opt => opt.Ignore())
            .ForMember(x => x.Preferences, opt => opt.Ignore());

        this.CreateMap<Device, ApiDevice>()
            .ForMember(x => x.State, opt => opt.MapFrom(x => ApiDeviceState.Offline));

        this.CreateMap<ApiSensor, Sensor>()
            .ForMember(x => x.IsKnown, opt => opt.MapFrom(x => false))
            .ForMember(x => x.Metadata, opt => opt.Ignore())
            .ForMember(x => x.SourceDevice, opt => opt.Ignore());

        this.CreateMap<Sensor, ApiSensor>();

        this.CreateMap<ApiActuator, Actuator>()
            .ForMember(x => x.IsKnown, opt => opt.MapFrom(x => false))
            .ForMember(x => x.SourceDevice, opt => opt.Ignore())
            .ForMember(x => x.Sensor, opt => opt.Ignore());

        this.CreateMap<Actuator, ApiActuator>();
    }
}
