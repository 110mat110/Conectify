namespace Conectify.Shared.Maps;

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

        CreateMap<MetadataConnector<Actuator>, ApiMetadata>()
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Metadata.Name));
        CreateMap<MetadataConnector<Sensor>, ApiMetadata>()
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Metadata.Name));
        CreateMap<MetadataConnector<Device>, ApiMetadata>()
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Metadata.Name));

        CreateMap<ApiBasicMetadata, Metadata>();
        CreateMap<Metadata, ApiBasicMetadata>();
    }
}
