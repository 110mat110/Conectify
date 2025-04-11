﻿namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;

public interface IActuatorService : IUniversalDeviceService<ApiActuator>
{
    Task<IEnumerable<ApiActuator>> GetAllActuatorsPerDevice(Guid deviceId, CancellationToken ct = default);
}

public class ActuatorService(ConectifyDb database, IMapper mapper, IDeviceService deviceService, ILogger<ActuatorService> logger, IHttpFactory httpFactory, Configuration configuration) : UniversalDeviceService<Actuator, ApiActuator>(database, mapper, logger, httpFactory, configuration), IActuatorService
{
    public override async Task<bool> TryAddUnknownDevice(Guid actuatorId, Guid deviceId, CancellationToken ct = default)
    {
        if (deviceId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(deviceId));
        }

        if (await database.Actuators.AsNoTracking().AnyAsync(x => x.Id == actuatorId, ct))
        {
            return false;
        }

        await deviceService.TryAddUnknownDevice(deviceId, deviceId, ct);

        var actuator = new Actuator()
        {
            Id = actuatorId,
            IsKnown = false,
            Name = "unknown Actuator",
            SourceDeviceId = deviceId,
        };

        await database.AddAsync(actuator, ct);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<ApiActuator>> GetAllActuatorsPerDevice(Guid deviceId, CancellationToken ct = default)
    {
        return await database.Actuators.AsNoTracking().Where(x => x.SourceDeviceId == deviceId).ProjectTo<ApiActuator>(mapper.ConfigurationProvider).ToListAsync(ct);
    }

    public override async Task<IEnumerable<ApiMetadata>> GetMetadata(Guid actuatorId, CancellationToken ct = default)
    {
        var actuator = await database.Set<Actuator>().FirstOrDefaultAsync(x => x.Id == actuatorId, ct);
        if (actuator is null)
        {
            return [];
        }
        var actuatorMetadatas = await database.Set<MetadataConnector<Actuator>>().Where(x => x.DeviceId == actuatorId).AsNoTracking().ProjectTo<ApiMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);

        var deviceMetadata = await database.Set<MetadataConnector<Device>>().Where(x => x.DeviceId == actuator.SourceDeviceId).AsNoTracking().ProjectTo<ApiMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);

        return actuatorMetadatas.Concat(deviceMetadata.Where(x => !actuatorMetadatas.Select(x => x.Name).Contains(x.Name)));
    }

    public override async Task<IEnumerable<ApiActuator>> Filter(ApiFilter filter, CancellationToken ct = default)
    {
        var exclude = new List<Guid>();
        if (filter.IsVisible)
        {
            exclude.AddRange(await database.Set<MetadataConnector<Actuator>>().AsNoTracking().Include(x => x.Metadata).Where(x => x.Metadata.Name == Constants.Metadatas.Visible && x.NumericValue == 0).Select(x => x.DeviceId).ToListAsync(ct));
        }

        if (!string.IsNullOrEmpty(filter.Name))
        {
            return await database.Set<Actuator>().AsNoTracking().Where(x => x.Name.Contains(filter.Name) && !exclude.Contains(x.Id)).ProjectTo<ApiActuator>(mapper.ConfigurationProvider).ToListAsync(ct);
        }

        return await database.Set<Actuator>().AsNoTracking().Where(x => !exclude.Contains(x.Id)).ProjectTo<ApiActuator>(mapper.ConfigurationProvider).ToListAsync(ct);
    }
}