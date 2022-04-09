namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

public interface IDeviceService
{
    Task<bool> AddKnownDevice(ApiDevice apiDevice, CancellationToken ct = default);
    Task<bool> AddUnknownDevice(Guid deviceId, CancellationToken ct = default);
    Task<ApiDevice?> GetSpecificDevice(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ApiDevice>> GetAllDevices(CancellationToken ct = default);
}

public class DeviceService : IDeviceService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly ILogger<DeviceService> logger;

    public DeviceService(ConectifyDb database, IMapper mapper, ILogger<DeviceService> logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<bool> AddKnownDevice(ApiDevice apiDevice, CancellationToken ct = default)
    {
        var device = mapper.Map<Device>(apiDevice);
        device.IsKnown = true;

        await database.AddOrUpdateAsync(device);        
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AddUnknownDevice(Guid deviceId, CancellationToken ct = default)
    {
        if (await database.Devices.AsNoTracking().AnyAsync(x => x.Id == deviceId, ct))
        {
            return false;
        }

        logger.LogError($"There is not device in dbs with id {deviceId}");

        var device = new Device()
        {
            Id = deviceId,
            IsKnown = false,
            MacAdress = string.Empty,
            IPAdress = string.Empty,
            Name = "unknown device",
            SubscibeToAll = false,
        };

        await database.AddAsync(device);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ApiDevice?> GetSpecificDevice(Guid id, CancellationToken ct = default)
    {
        var dbsModel = await database.Devices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        if (dbsModel == null)
            return null;

        return mapper.Map<ApiDevice>(dbsModel);
    }

    public async Task<IEnumerable<ApiDevice>> GetAllDevices(CancellationToken ct = default)
    {
        return await database.Devices.AsNoTracking().ProjectTo<ApiDevice>(mapper.ConfigurationProvider).ToListAsync(ct);
    }
}
