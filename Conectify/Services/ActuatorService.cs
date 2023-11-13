namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;

public interface IActuatorService : IUniversalDeviceService<ApiActuator>
{
    Task<IEnumerable<ApiActuator>> GetAllActuatorsPerDevice(Guid deviceId, CancellationToken ct = default);
}

public class ActuatorService : UniversalDeviceService<Actuator, ApiActuator>, IActuatorService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly IDeviceService deviceService;

    public ActuatorService(ConectifyDb database, IMapper mapper, IDeviceService deviceService, ILogger<ActuatorService> logger, IHttpFactory httpFactory, Configuration configuration) : base(database, mapper, logger, httpFactory, configuration)
	{
        this.database = database;
        this.mapper = mapper;
        this.deviceService = deviceService;
    }

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

        await database.AddAsync(actuator);
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
            return new List<ApiMetadata>();
        }
        var actuatorMetadatas = await database.Set<MetadataConnector<Actuator>>().Where(x => x.DeviceId == actuatorId).AsNoTracking().ProjectTo<ApiMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);

        var deviceMetadata = await database.Set<MetadataConnector<Device>>().Where(x => x.DeviceId == actuator.SourceDeviceId).AsNoTracking().ProjectTo<ApiMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);

        return actuatorMetadatas.Concat(deviceMetadata.Where(x => !actuatorMetadatas.Select(x => x.Name).Contains(x.Name)));
    }
}