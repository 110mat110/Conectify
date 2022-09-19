namespace Conectify.Server.Services;

using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

public interface IDeviceService : IUniversalDeviceService<ApiDevice>
{
}

public class DeviceService : UniversalDeviceService<Device, ApiDevice>, IDeviceService
{
    private readonly ConectifyDb database;
    private readonly ILogger<DeviceService> logger;

    public DeviceService(ConectifyDb database, IMapper mapper, ILogger<DeviceService> logger) : base(database, mapper, logger)
    {
        this.database = database;
        this.logger = logger;
    }

    public override async Task<bool> TryAddUnknownDevice(Guid deviceId, Guid parentId = default, CancellationToken ct = default)
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
            SubscribeToAll = false,
        };

        await database.AddAsync(device);
        await database.SaveChangesAsync(ct);
        return true;
    }
}
