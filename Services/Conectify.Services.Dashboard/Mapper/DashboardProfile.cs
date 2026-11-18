using Conectify.Database.Models.Dashboard;
using Conectify.Services.Dashboard.Models;
using Conectify.Shared.Library.Mapping;

namespace Conectify.Services.Dashboard.Mapper;

public class DashboardProfile : IMappingProfile
{
    public void Configure(MapperConfiguration configuration)
    {
        configuration.CreateMap<AddDashboardApi, Database.Models.Dashboard.Dashboard>(
            x => new Database.Models.Dashboard.Dashboard
            {
                Id = Guid.NewGuid(),
                UserId = x.UserId,
                Position = x.Position,
                Type = x.Type,
                Background = string.Empty,
            },
            nameof(Database.Models.Dashboard.Dashboard.Name), nameof(Database.Models.Dashboard.Dashboard.User));

        configuration.CreateMap<AddDeviceApi, DashboardDevice>(
            x => new DashboardDevice
            {
                Id = Guid.NewGuid(),
                DeviceId = x.DeviceId,
                PosX = x.PosX,
                PosY = x.PosY,
                SourceType = x.SourceType,
            },
            nameof(DashboardDevice.DashBoardId));

        configuration.CreateMap<Database.Models.Dashboard.Dashboard, DashboardApi>(
            x => new DashboardApi
            {
                Id = x.Id,
                Background = x.Background,
                Name = x.Name,
                Type = (int)x.Type,
            },
            nameof(DashboardApi.DashboardDevices));

        configuration.CreateMap<DashboardDevice, DashboardDeviceApi>(x => new DashboardDeviceApi
        {
            Id = x.Id,
            DeviceId = x.DeviceId,
            PosX = x.PosX,
            PosY = x.PosY,
            SourceType = x.SourceType,
        });
    }
}
