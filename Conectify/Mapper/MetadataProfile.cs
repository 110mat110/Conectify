namespace Conectify.Server.Mapper;

using AutoMapper;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;

public class MetadataProfile : Profile
{

    public MetadataProfile()
    {
        CreateMap<Metadata, ApiMetadata>();

        CreateMap<ApiMetadata, Metadata>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Actuator, opt => opt.Ignore())
            .ForMember(x => x.Sensor, opt => opt.Ignore())
            .ForMember(x => x.Device, opt => opt.Ignore())
            .ForMember(x => x.SensorId, opt => opt.Ignore())
            .ForMember(x => x.ActuatorId, opt => opt.Ignore())
            .ForMember(x => x.DeviceId, opt => opt.Ignore());
    }
}
