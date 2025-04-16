namespace Conectify.Server.Services;

using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;

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
        //await NotifyAboutNewDevice(ct);
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

        var metada = await database.Set<Metadata>().FirstOrDefaultAsync(x => x.Id == apiModel.MetadataId, ct);
        var incomingMetadata = mapper.Map<MetadataConnector<TDbs>>(apiModel);
        var existingMetadataList = await database.Set<MetadataConnector<TDbs>>().AsNoTracking().Where(x => x.Id == apiModel.Id || (x.MetadataId == apiModel.MetadataId && x.DeviceId == apiModel.DeviceId)).ToListAsync(cancellationToken: ct);

        if (existingMetadataList.Count > 0 && metada?.Exclusive is false)
        {
            if (existingMetadataList.Any(range =>
            (incomingMetadata.MinVal ?? int.MinValue) <= (range.MaxVal ?? int.MaxValue) &&
            (incomingMetadata.MaxVal ?? int.MaxValue) >= (range.MinVal ?? int.MinValue)))
            {
                return false;
            }

            await database.AddAsync(incomingMetadata, ct);
        }
        else if (existingMetadataList is null || existingMetadataList.Count == 0 || metada?.Exclusive is false)
        {
            await database.AddAsync(incomingMetadata, ct);
        }
        else
        {
            var existingMetadata = existingMetadataList.First();
            existingMetadata.NumericValue = incomingMetadata.NumericValue;
            existingMetadata.Unit = incomingMetadata.Unit;
            existingMetadata.StringValue = incomingMetadata.StringValue;
            existingMetadata.MinVal = incomingMetadata.MinVal;
            existingMetadata.MaxVal = incomingMetadata.MaxVal;
            existingMetadata.TypeValue = incomingMetadata.TypeValue;

            database.Update(existingMetadata);
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
