namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

public interface IActuatorService
{
    Task<bool> AddKnownActuator(ApiActuator apiActuator, CancellationToken ct = default);
    Task<bool> AddUnknownActuator(Guid ActuatorId, Guid deviceId, CancellationToken ct = default);
    Task<IEnumerable<ApiActuator>> GetAllActuatorsPerDevice(Guid deviceId, CancellationToken ct = default);
    Task<ApiActuator?> GetActuator(Guid id, CancellationToken ct = default);
}

public class ActuatorService : IActuatorService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly IDeviceService deviceService;

    public ActuatorService(ConectifyDb database, IMapper mapper, IDeviceService deviceService)
    {
        this.database = database;
        this.mapper = mapper;
        this.deviceService = deviceService;
    }

    public async Task<bool> AddKnownActuator(ApiActuator apiActuator, CancellationToken ct = default)
    {
        var device = mapper.Map<Actuator>(apiActuator);
        device.IsKnown = true;

        await database.AddOrUpdateAsync(device);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AddUnknownActuator(Guid actuatorId, Guid deviceId, CancellationToken ct = default)
    {
        if (await database.Actuators.AsNoTracking().AnyAsync(x => x.Id == actuatorId, ct))
        {
            return false;
        }

        await deviceService.AddUnknownDevice(deviceId, ct);

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

    public async Task<ApiActuator?> GetActuator(Guid id, CancellationToken ct = default)
    {
        var dbsModel = await database.Actuators.AsNoTracking().Include(i => i.Metadata).FirstOrDefaultAsync(x => x.Id == id, ct);

        if (dbsModel == null)
            return null;

        return mapper.Map<ApiActuator>(dbsModel);
    }

    public async Task<IEnumerable<ApiActuator>> GetAllActuatorsPerDevice(Guid deviceId, CancellationToken ct = default)
    {
        return await database.Actuators.AsNoTracking().Where(x => x.SourceDeviceId == deviceId).ProjectTo<ApiActuator>(mapper.ConfigurationProvider).ToListAsync(ct);
    }
}
