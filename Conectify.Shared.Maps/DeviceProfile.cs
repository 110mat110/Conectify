namespace Conectify.Shared.Maps;

using Conectify.Database.Models;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Library.Models;

public class DeviceProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        configuration.CreateMap<ApiDevice, Device>(
            x => new Device
            {
                Id = x.Id,
                IPAdress = x.IPAdress,
                MacAdress = x.MacAdress,
                Name = x.Name,
                IsKnown = false,
                SubscribeToAll = false,
            },
            nameof(Device.Metadata), nameof(Device.Sensors), nameof(Device.Actuators), nameof(Device.Preferences), nameof(Device.DeviceVersions));

        configuration.CreateMap<Device, ApiDevice>(x => new ApiDevice
        {
            Id = x.Id,
            IPAdress = x.IPAdress,
            MacAdress = x.MacAdress,
            Name = x.Name,
            State = ApiDeviceState.Offline,
            Metadata = x.Metadata.Select(m => new ApiMetadata
            {
                Id = m.Id,
                Name = m.Metadata.Name,
                NumericValue = m.NumericValue,
                StringValue = m.StringValue,
                Unit = m.Unit,
                MinVal = m.MinVal,
                MaxVal = m.MaxVal,
                MetadataId = m.MetadataId,
            }).ToList(),
        });

        configuration.CreateMap<ApiSensor, Sensor>(
            x => new Sensor
            {
                Id = x.Id,
                Name = x.Name,
                SourceDeviceId = x.SourceDeviceId,
                IsKnown = false,
            },
            nameof(Sensor.Metadata), nameof(Sensor.SourceDevice));

        configuration.CreateMap<Sensor, ApiSensor>(x => new ApiSensor
        {
            Id = x.Id,
            Name = x.Name,
            SourceDeviceId = x.SourceDeviceId,
            Metadata = x.Metadata.Select(m => new ApiMetadata
            {
                Id = m.Id,
                Name = m.Metadata.Name,
                NumericValue = m.NumericValue,
                StringValue = m.StringValue,
                Unit = m.Unit,
                MinVal = m.MinVal,
                MaxVal = m.MaxVal,
                MetadataId = m.MetadataId,
            }).ToList(),
        });

        configuration.CreateMap<ApiActuator, Actuator>(
            x => new Actuator
            {
                Id = x.Id,
                Name = x.Name,
                SourceDeviceId = x.SourceDeviceId,
                SensorId = x.SensorId,
                IsKnown = false,
                Metadata = (x.Metadata ?? new List<ApiMetadata>()).Select(m => new MetadataConnector<Actuator>
                {
                    Id = m.Id,
                    NumericValue = m.NumericValue,
                    StringValue = m.StringValue,
                    Unit = m.Unit,
                    MinVal = m.MinVal,
                    MaxVal = m.MaxVal,
                    MetadataId = m.MetadataId,
                }).ToList(),
            },
            nameof(Actuator.SourceDevice), nameof(Actuator.Sensor));

        configuration.CreateMap<Actuator, ApiActuator>(x => new ApiActuator
        {
            Id = x.Id,
            Name = x.Name,
            SourceDeviceId = x.SourceDeviceId,
            SensorId = x.SensorId,
            Metadata = x.Metadata.Select(m => new ApiMetadata
            {
                Id = m.Id,
                Name = m.Metadata.Name,
                NumericValue = m.NumericValue,
                StringValue = m.StringValue,
                Unit = m.Unit,
                MinVal = m.MinVal,
                MaxVal = m.MaxVal,
                MetadataId = m.MetadataId,
            }).ToList(),
        });
    }
}
