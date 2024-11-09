/* This code is defining a C# class called `DeviceService` that implements the `IDeviceService`
interface. */
namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Server.Caches;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public interface IDeviceService : IUniversalDeviceService<ApiDevice>
{
}

public class DeviceService(ConectifyDb database, IMapper mapper, ILogger<DeviceService> logger, IHttpFactory httpFactory, Configuration configuration, IWebsocketCache websocketCache) : UniversalDeviceService<Device, ApiDevice>(database, mapper, logger, httpFactory, configuration), IDeviceService
{
    public override async Task<IEnumerable<ApiDevice>> Filter(ApiFilter filter, CancellationToken ct = default)
    {
        if (filter.IsVisible)
        {
            filter.MetadataFilters = filter.MetadataFilters.Append(new ApiMetadataFilter() { Name = Constants.Metadatas.Visible, NumericValue = 1, EqualityComparator = false });
        }

        var set = database.Set<Device>().AsNoTracking().Include(x => x.Metadata).ThenInclude(x => x.Metadata).AsQueryable();
        if (filter.MetadataFilters.Any())
        {
            foreach(var metadata in filter.MetadataFilters)
            {
                set = set.Where(x => 
                    x.Metadata.Any(m =>
                        m.Metadata.Name == metadata.Name && 
                        (string.IsNullOrEmpty(metadata.Value) && (metadata.EqualityComparator && m.StringValue == metadata.Value) ||
                        metadata.NumericValue != null && m.NumericValue == metadata.NumericValue
                        )));
            }
        }

        if (!string.IsNullOrEmpty(filter.Name))
        {
            return await set.Where(x => x.Name.Contains(filter.Name)).ProjectTo<ApiDevice>(mapper.ConfigurationProvider).ToListAsync(ct);
        }

        return await set.ProjectTo<ApiDevice>(mapper.ConfigurationProvider).ToListAsync(ct);
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

    public override async Task<IEnumerable<ApiDevice>> GetAllDevices(CancellationToken ct = default)
    {
        var devices = await base.GetAllDevices(ct);
        var currentTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)).ToUnixTimeMilliseconds();
        foreach (var device in devices)
        {
            if (!websocketCache.IsActiveSocket(device.Id))
            {
                device.State = ApiDeviceState.Offline;
            }
            else
            {
                if (await database.Events.AsNoTracking().AnyAsync(
                    x => x.Name == Constants.Commands.Active 
                    && x.Type == Constants.Events.CommandResponse
                    && x.SourceId == device.Id 
                    && x.TimeCreated > currentTime, cancellationToken: ct))
                {
                    device.State = ApiDeviceState.Online;
                }
                else
                {
                    device.State = ApiDeviceState.NotAnswering;
                }
            }
        }

        return devices;
    }
}
