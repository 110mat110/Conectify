/* This code is defining a C# class called `DeviceService` that implements the `IDeviceService`
interface. */
namespace Conectify.Server.Services;

using System.Collections.Generic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Server.Caches;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;

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
            foreach (var metadata in filter.MetadataFilters)
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

        logger.LogError("There is not device in dbs with id {deviceId}", deviceId);

        var device = new Device()
        {
            Id = deviceId,
            IsKnown = false,
            MacAdress = string.Empty,
            IPAdress = string.Empty,
            Name = "unknown device",
            SubscribeToAll = false,
        };

        await database.AddAsync(device, ct);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public override async Task<IEnumerable<ApiDevice>> GetAllDevices(CancellationToken ct = default)
    {
        var devices = await base.GetAllDevices(ct);
        var currentTime = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)).ToUnixTimeMilliseconds();
        
        var allCommands = await database.Events.Where(e => e.Type == "Command" && e.Name == "activitycheck" && e.TimeCreated > currentTime).ToListAsync();
        List<IEnumerable<Database.Models.Values.Event>>? allResponses = await (from cmd in database.Events
        .Where(e => e.Type == Constants.Events.Command && e.Name == Constants.Commands.ActivityCheck && e.TimeCreated > currentTime)
                              join resp in database.Events
                                  .Where(e => e.Name == Constants.Commands.Active)
                                  on cmd.Id equals resp.SourceId into responses
                              select 
                                  responses
                            ).ToListAsync(ct);


        var activeDevices = await (from cmd in database.Events
        .Where(e => e.Type == Constants.Events.Command && e.Name == Constants.Commands.ActivityCheck && e.TimeCreated > currentTime)
                            join resp in database.Events
                                .Where(e => e.Name ==Constants.Commands.Active)
                                on cmd.Id equals resp.SourceId into responses
                            select 
                                cmd.DestinationId
                            ).Distinct().ToListAsync(ct);
        foreach (var device in devices)
        {
            device.State = !websocketCache.IsActiveSocket(device.Id)
                ? ApiDeviceState.Offline
                : activeDevices.Contains(device.Id) ? ApiDeviceState.Online : ApiDeviceState.NotAnswering;
        }

        return devices.OrderByDescending(x => x.State);
    }
}
