using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Library.Models;

namespace Conectify.Services.Automatization.Mapper;

public class ActuatorProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        configuration.CreateMap<AddActuatorApiModel, ApiActuator>(
            x => new ApiActuator
            {
                Id = Guid.NewGuid(),
                Name = x.ActuatorName,
            },
            nameof(ApiActuator.Metadata), nameof(ApiActuator.SourceDeviceId), nameof(ApiActuator.SensorId));
    }
}
