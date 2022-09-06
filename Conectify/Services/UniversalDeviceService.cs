namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

public interface IUniversalDeviceService<TApi>
{
    Task<Guid> AddKnownDevice(TApi apiDevice, CancellationToken ct = default);
    Task<bool> TryAddUnknownDevice(Guid deviceId, Guid parentId = default, CancellationToken ct = default);
    Task<IEnumerable<TApi>> GetAllDevices(CancellationToken ct = default);
    Task<TApi?> GetSpecificDevice(Guid id, CancellationToken ct = default);
    Task<bool> AddMetadata(ApiMetadataConnector apiModel, CancellationToken ct = default);
    Task<IEnumerable<ApiMetadataConnector>> GetMetadata(Guid deviceId, CancellationToken ct = default);
}

public abstract class UniversalDeviceService<TDbs, TApi> : IUniversalDeviceService<TApi> where TDbs : class, IEntity, IDevice
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly ILogger<UniversalDeviceService<TDbs, TApi>> logger;

    public UniversalDeviceService(ConectifyDb database, IMapper mapper, ILogger<UniversalDeviceService<TDbs, TApi>> logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<Guid> AddKnownDevice(TApi apiDevice, CancellationToken ct = default)
    {
        var device = mapper.Map<TDbs>(apiDevice);
        device.IsKnown = true;

        await database.AddOrUpdateAsync(device);
        await database.SaveChangesAsync(ct);
        return device.Id;
    }

    public abstract Task<bool> TryAddUnknownDevice(Guid deviceId, Guid parentId = default, CancellationToken ct = default);

    public async Task<TApi?> GetSpecificDevice(Guid id, CancellationToken ct = default)
    {
        var dbsModel = await database.Devices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        if (dbsModel == null)
            return default;

        return mapper.Map<TApi>(dbsModel);
    }

    public async Task<IEnumerable<TApi>> GetAllDevices(CancellationToken ct = default)
    {
        return await database.Devices.AsNoTracking().ProjectTo<TApi>(mapper.ConfigurationProvider).ToListAsync(ct);
    }

    public async Task<bool> AddMetadata(ApiMetadataConnector apiModel, CancellationToken ct = default)
    {
        if (await database.Set<TDbs>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == apiModel.DeviceId) is null || await database.Set<Metadata>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == apiModel.MetadataId) is null)
        {
            logger.LogWarning("Not found device or metadata for import");
            return false;
        }

        var mapedModel = mapper.Map<MetadataConnector<TDbs>>(apiModel);
        var model = await database.Set<MetadataConnector<TDbs>>().FirstOrDefaultAsync(x => x.MetadataId == apiModel.MetadataId && x.DeviceId == apiModel.DeviceId, ct);

        if (model is null)
        {
            await database.AddAsync(mapedModel);
        }
        else
        {
            model = mapedModel;
            database.Update(model);
        }

        await database.SaveChangesAsync(ct);

        return true;
    }

    public async Task<IEnumerable<ApiMetadataConnector>> GetMetadata(Guid deviceId, CancellationToken ct = default)
    {
        return await database.Set<MetadataConnector<TDbs>>().Where(x => x.DeviceId == deviceId).AsNoTracking().ProjectTo<ApiMetadataConnector>(mapper.ConfigurationProvider).ToListAsync(ct);
    }
}
