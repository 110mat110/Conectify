namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public interface IDeviceService : IUniversalDeviceService<ApiDevice>
{
}

public class DeviceService : UniversalDeviceService<Device, ApiDevice>, IDeviceService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly ILogger<DeviceService> logger;

    public DeviceService(ConectifyDb database, IMapper mapper, ILogger<DeviceService> logger, IHttpFactory httpFactory, Configuration configuration) : base(database, mapper, logger, httpFactory, configuration)
	{
        this.database = database;
        this.mapper = mapper;
        this.logger = logger;
    }

    public override async Task<IEnumerable<ApiDevice>> Filter(ApiFilter filter, CancellationToken ct = default)
    {
        var exclude = new List<Guid>();
        if (filter.IsVisible)
        {
            exclude.AddRange(await database.Set<MetadataConnector<Device>>().AsNoTracking().Include(x => x.Metadata).Where(x => x.Metadata.Name == Constants.Metadatas.Visible && x.NumericValue == 0).Select(x => x.DeviceId).ToListAsync(ct));
        }

        if (!string.IsNullOrEmpty(filter.Name))
        {
            return await database.Set<Device>().AsNoTracking().Where(x => x.Name.Contains(filter.Name) && !exclude.Contains(x.Id)).ProjectTo<ApiDevice>(mapper.ConfigurationProvider).ToListAsync(ct);
        }

        return await database.Set<Device>().AsNoTracking().Where(x => !exclude.Contains(x.Id)).ProjectTo<ApiDevice>(mapper.ConfigurationProvider).ToListAsync(ct);
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
