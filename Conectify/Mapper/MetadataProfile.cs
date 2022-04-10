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
            .ForMember(x => x.Id, opt => opt.Ignore());

        CreateMap<ApiMetadataConnector, MetadataConnector<Device>>()
            .ForMember(x => x.Metadata, opt => opt.Ignore())
            .ForMember(x => x.Device, opt => opt.Ignore());

        CreateMap<ApiMetadataConnector, MetadataConnector<Actuator>>()
            .ForMember(x => x.Metadata, opt => opt.Ignore())
            .ForMember(x => x.Device, opt => opt.Ignore());

        CreateMap<ApiMetadataConnector, MetadataConnector<Sensor>>()
            .ForMember(x => x.Metadata, opt => opt.Ignore())
            .ForMember(x => x.Device, opt => opt.Ignore());

        CreateMap<MetadataConnector<Device>, ApiMetadataConnector>();
        CreateMap<MetadataConnector<Sensor>, ApiMetadataConnector>();
        CreateMap<MetadataConnector<Actuator>, ApiMetadataConnector>();
    }
}
