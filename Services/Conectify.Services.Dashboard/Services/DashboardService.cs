using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models.Dashboard;
using Conectify.Services.Dashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Services.Dashboard.Services;

public class DashboardService(ConectifyDb conectifyDb, IMapper mapper)
{
    public async Task<DashboardApi> Add(AddDashboardApi addDashboardApi, CancellationToken cancellationToken = default)
    {
        if (!conectifyDb.Users.Any(x => x.Id == addDashboardApi.UserId))
        {
            throw new ArgumentException("User with id {id} does not exist", addDashboardApi.UserId.ToString());
        }
        var dashboard = mapper.Map<Database.Models.Dashboard.Dashboard>(addDashboardApi);

        var entity = await conectifyDb.AddOrUpdateAsync(dashboard, cancellationToken);
        await conectifyDb.SaveChangesAsync(cancellationToken);
        return mapper.Map<DashboardApi>(entity);
    }

    public async Task<IEnumerable<DashboardApi>> GetDashboards(Guid userId, CancellationToken cancellationToken = default)
    {
        var dashboardsDbs = await conectifyDb.Dashboards.Where(x => x.UserId == userId).OrderBy(x => x.Position).ToListAsync(cancellationToken);
        var dashboards = mapper.Map<IEnumerable<DashboardApi>>(dashboardsDbs);

        foreach (var dashboard in dashboards)
        {
            var devices = await conectifyDb.DashboardsDevice.Where(x => x.DashBoardId == dashboard.Id).ProjectTo<DashboardDeviceApi>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);
            dashboard.DashboardDevices = devices;
        }

        return dashboards;
    }

    public async Task<DashboardApi> GetDashboard(Guid dashboardId, CancellationToken cancellationToken = default)
    {
        var dbModel = await conectifyDb.Dashboards.FirstOrDefaultAsync(x => x.Id == dashboardId, cancellationToken) ?? throw new ArgumentException("Selected dashboard does not exist");
        var dashboard = mapper.Map<DashboardApi>(dbModel);

        var devices = await conectifyDb.DashboardsDevice.Where(x => x.DashBoardId == dashboardId).ProjectTo<DashboardDeviceApi>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        dashboard.DashboardDevices = devices;

        return dashboard;
    }

    public async Task Remove(Guid id, CancellationToken cancellationToken = default)
    {
        var devices = conectifyDb.DashboardsDevice.Where(x => x.DashBoardId == id).ToListAsync(cancellationToken);
        conectifyDb.RemoveRange(devices);

        var dashboard = conectifyDb.Dashboards.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (dashboard != null)
        {
            conectifyDb.Remove(dashboard);
        }

        await conectifyDb.SaveChangesAsync(cancellationToken);
    }

    public async Task Edit(Guid id, EditDashboardApi editDashboardApi, CancellationToken cancellationToken = default)
    {
        var dashboard = await conectifyDb.Dashboards.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new ArgumentException("Selected dashboard does not exist");

        dashboard.Background = editDashboardApi.Background;
        dashboard.Name = editDashboardApi.Name;
        conectifyDb.Update(dashboard);
        await conectifyDb.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> AddDevice(Guid dashboardId, AddDeviceApi deviceApi, CancellationToken cancellationToken = default)
    {
        var device = mapper.Map<DashboardDevice>(deviceApi);
        device.DashBoardId = dashboardId;

        await conectifyDb.DashboardsDevice.AddAsync(device, cancellationToken);
        await conectifyDb.SaveChangesAsync(cancellationToken);

        return device.Id;
    }

    public async Task EditDevice(EditDeviceApi deviceApi, CancellationToken cancellationToken = default)
    {
        var device = await conectifyDb.DashboardsDevice.FirstOrDefaultAsync(x => x.Id == deviceApi.Id, cancellationToken) ?? throw new ArgumentException("Device with id {id} does not exist", deviceApi.Id.ToString());

        device.PosX = deviceApi.PosX;
        device.PosY = deviceApi.PosY;

        await conectifyDb.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveDevice(Guid deviceId, CancellationToken cancellationToken = default)
    {
        var device = await conectifyDb.DashboardsDevice.FirstOrDefaultAsync(x => x.Id == deviceId, cancellationToken) ?? throw new ArgumentException("Device with id {id} does not exist", deviceId.ToString());

        conectifyDb.DashboardsDevice.Remove(device);

        await conectifyDb.SaveChangesAsync(cancellationToken);
    }
}
