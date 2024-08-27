
using AutoMapper;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Shared.Library.Models;

namespace Conectify.Services.Automatization.Mapper;

public class ActuatorProfile : Profile
{
    public ActuatorProfile()
    {
        this.CreateMap<AddActuatorApiModel, ApiActuator>()
            .ForMember(x => x.Metadata, opt => opt.Ignore())
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.ActuatorName))
            .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
            .ForMember(x => x.SourceDeviceId, opt => opt.Ignore())
            .ForMember(x => x.SensorId, opt => opt.Ignore());

    }
}
