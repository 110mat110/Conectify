namespace Conectify.Shared.Maps;

using Conectify.Database.Models;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Services;

public class MetadataProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        AddConnectorMaps<Device>(configuration);
        AddConnectorMaps<Sensor>(configuration);
        AddConnectorMaps<Actuator>(configuration);

        configuration.CreateMap<ApiMetadata, MetadataConnector<Actuator>>(
            x => new MetadataConnector<Actuator>
            {
                Id = x.Id,
                NumericValue = x.NumericValue,
                StringValue = x.StringValue,
                Unit = x.Unit,
                MinVal = x.MinVal,
                MaxVal = x.MaxVal,
                MetadataId = x.MetadataId,
            },
            nameof(MetadataConnector<Actuator>.DeviceId), nameof(MetadataConnector<Actuator>.TypeValue), nameof(MetadataConnector<Actuator>.Metadata), nameof(MetadataConnector<Actuator>.Device));

        configuration.CreateMap<MetadataServiceConnector, ApiMetadataConnector>(
            x => new ApiMetadataConnector
            {
                Id = x.Id,
                NumericValue = x.NumericValue,
                StringValue = x.StringValue,
                TypeValue = x.TypeValue,
                Unit = x.Unit,
                MinVal = x.MinVal,
                MaxVal = x.MaxVal,
            },
            nameof(ApiMetadataConnector.MetadataId), nameof(ApiMetadataConnector.DeviceId));

        configuration.CreateMap<ApiBasicMetadata, Metadata>(x => new Metadata
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Exclusive = x.Exclusive,
        });

        configuration.CreateMap<Metadata, ApiBasicMetadata>(x => new ApiBasicMetadata
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Exclusive = x.Exclusive,
        });
    }

    private static void AddConnectorMaps<T>(MapperConfiguration configuration)
    {
        configuration.CreateMap<ApiMetadataConnector, MetadataConnector<T>>(
            x => new MetadataConnector<T>
            {
                Id = x.Id,
                NumericValue = x.NumericValue,
                StringValue = x.StringValue,
                TypeValue = x.TypeValue,
                Unit = x.Unit,
                MinVal = x.MinVal,
                MaxVal = x.MaxVal,
                MetadataId = x.MetadataId,
                DeviceId = x.DeviceId,
            },
            nameof(MetadataConnector<T>.Metadata), nameof(MetadataConnector<T>.Device));

        configuration.CreateMap<MetadataConnector<T>, ApiMetadataConnector>(x => new ApiMetadataConnector
        {
            Id = x.Id,
            NumericValue = x.NumericValue,
            StringValue = x.StringValue,
            TypeValue = x.TypeValue,
            Unit = x.Unit,
            MinVal = x.MinVal,
            MaxVal = x.MaxVal,
            MetadataId = x.MetadataId,
            DeviceId = x.DeviceId,
        });

        configuration.CreateMap<MetadataConnector<T>, ApiMetadata>(x => new ApiMetadata
        {
            Id = x.Id,
            Name = x.Metadata.Name,
            NumericValue = x.NumericValue,
            StringValue = x.StringValue,
            Unit = x.Unit,
            MinVal = x.MinVal,
            MaxVal = x.MaxVal,
            MetadataId = x.MetadataId,
        });
    }
}
