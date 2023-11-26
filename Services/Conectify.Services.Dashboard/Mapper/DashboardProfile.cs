using AutoMapper;
using Conectify.Database.Models.Dashboard;
using Conectify.Services.Dashboard.Models;

namespace Conectify.Services.Dashboard.Mapper;

public class DashboardProfile : Profile
{
    public DashboardProfile()
    {
        this.CreateMap<AddDashboardApi, Database.Models.Dashboard.Dashboard>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
            .ForMember(x => x.User, opt => opt.Ignore())
            .ForMember(x => x.Background, opt => opt.MapFrom(x => string.Empty));

        this.CreateMap<AddDeviceApi, DashboardDevice>()
    .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
    .ForMember(x => x.DashBoardId, opt => opt.Ignore());

        this.CreateMap<Database.Models.Dashboard.Dashboard, DashboardApi>()
            .ForMember(x => x.DashboradDevices, opt => opt.MapFrom(x => new List<DashboardDeviceApi>()));

    }
}
