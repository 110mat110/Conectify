namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

public interface IUniversalDeviceService<TApi>
{
    Task<Guid> AddKnownDevice(TApi apiDevice, CancellationToken ct = default);
    Task<bool> TryAddUnknownDevice(Guid deviceId, Guid parentId = default, CancellationToken ct = default);
    Task<IEnumerable<TApi>> GetAllDevices(CancellationToken ct = default);
    Task<TApi?> GetSpecificDevice(Guid id, CancellationToken ct = default);
    Task<bool> AddMetadata(ApiMetadataConnector apiModel, CancellationToken ct = default);
    Task<IEnumerable<ApiMetadata>> GetMetadata(Guid deviceId, CancellationToken ct = default);
    Task<IEnumerable<TApi>> Filter(ApiFilter filter, CancellationToken ct = default);
}

public abstract class UniversalDeviceService<TDbs, TApi>(ConectifyDb database, IMapper mapper, ILogger<UniversalDeviceService<TDbs, TApi>> logger, IHttpFactory httpProvider, Configuration configuration) : IUniversalDeviceService<TApi> where TDbs : class, IEntity, IDevice
{
    public async Task<Guid> AddKnownDevice(TApi apiDevice, CancellationToken ct = default)
    {
        var device = mapper.Map<TDbs>(apiDevice);
        device.IsKnown = true;
        await NotifyAboutNewDevice(ct);
        await database.AddOrUpdateAsync(device, ct);
        await database.SaveChangesAsync(ct);
        return device.Id;
    }

    private async Task NotifyAboutNewDevice(CancellationToken ct)
    {
		using var client = httpProvider.HttpClient;
		await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, configuration.HistoryService + "/api/device/reset"), ct);
	}

    public abstract Task<bool> TryAddUnknownDevice(Guid deviceId, Guid parentId = default, CancellationToken ct = default);

    public async Task<TApi?> GetSpecificDevice(Guid id, CancellationToken ct = default)
    {
        var dbsModel = await database.Set<TDbs>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        if (dbsModel is null)
            return default;

        return mapper.Map<TApi>(dbsModel);
    }

    public virtual async Task<IEnumerable<TApi>> GetAllDevices(CancellationToken ct = default)
    {
        return await database.Set<TDbs>().AsNoTracking().ProjectTo<TApi>(mapper.ConfigurationProvider).ToListAsync(ct);
    }

    public async Task<bool> AddMetadata(ApiMetadataConnector apiModel, CancellationToken ct = default)
    {
        if (await database.Set<TDbs>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == apiModel.DeviceId, cancellationToken: ct) is null || await database.Set<Metadata>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == apiModel.MetadataId, cancellationToken: ct) is null)
        {
            logger.LogWarning("Not found device or metadata for import");
            return false;
        }

        var mapedModel = mapper.Map<MetadataConnector<TDbs>>(apiModel);
        var model = await database.Set<MetadataConnector<TDbs>>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == apiModel.Id || (x.MetadataId == apiModel.MetadataId && x.DeviceId == apiModel.DeviceId), ct);

        if (model is null)
        {
            await database.AddAsync(mapedModel, ct);
        }
        else
        {
            model.NumericValue = mapedModel.NumericValue;
            model.Unit = mapedModel.Unit;
            model.StringValue = mapedModel.StringValue;
            model.MinVal = mapedModel.MinVal;
            model.MaxVal = mapedModel.MaxVal;
            model.TypeValue = mapedModel.TypeValue;
            
            database.Update(model);
        }

        await database.SaveChangesAsync(ct);

        return true;
    }

    public virtual async Task<IEnumerable<ApiMetadata>> GetMetadata(Guid deviceId, CancellationToken ct = default)
    {
        return await database.Set<MetadataConnector<TDbs>>().Where(x => x.DeviceId == deviceId).AsNoTracking().ProjectTo<ApiMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);
    }

    public abstract Task<IEnumerable<TApi>> Filter(ApiFilter filter, CancellationToken ct = default);
}
