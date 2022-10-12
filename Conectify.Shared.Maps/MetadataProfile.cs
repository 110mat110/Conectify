namespace Conectify.Shared.Maps;

using AutoMapper;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;

public class MetadataProfile : Profile
{

    public MetadataProfile()
    {
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
        CreateMap<MetadataServiceConnector, ApiMetadataConnector>()
            .ForMember(x => x.MetadataId, opt => opt.Ignore())
            .ForMember(x => x.DeviceId, opt => opt.Ignore());

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
