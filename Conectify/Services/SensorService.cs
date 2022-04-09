namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

public interface ISensorService
{
    Task<bool> AddKnownSensor(ApiSensor apiSensor, CancellationToken ct = default);
    Task<bool> AddUnknownSensor(Guid sensorId, Guid deviceId, CancellationToken ct = default);
    Task<IEnumerable<ApiSensor>> GetAllSensorsPerDevice(Guid deviceId, CancellationToken ct = default);
    Task<ApiSensor?> GetSensor(Guid id, CancellationToken ct = default);
    Task<ApiSensor?> GetSensorByActuator(Guid id, CancellationToken ct = default);
}

public class SensorService : ISensorService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly IDeviceService deviceService;
    private readonly ILogger<SensorService> logger;

    public SensorService(ConectifyDb database, IMapper mapper, IDeviceService deviceService, ILogger<SensorService> logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.deviceService = deviceService;
        this.logger = logger;
    }

    public async Task<bool> AddKnownSensor(ApiSensor apiSensor, CancellationToken ct = default)
    {
        var device = mapper.Map<Sensor>(apiSensor);
        device.IsKnown = true;

        await database.AddOrUpdateAsync(device);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AddUnknownSensor(Guid sensorId, Guid deviceId, CancellationToken ct = default)
    {
        if (await database.Sensors.AsNoTracking().AnyAsync(x => x.Id == sensorId, ct))
        {
            return false;
        }

        logger.LogError($"There is not sensor in dbs with id {sensorId}");
        await deviceService.AddUnknownDevice(deviceId, ct);

        var sensor = new Sensor()
        {
            Id = sensorId,
            IsKnown = false,
            Name = "unknown sensor",
            SourceDeviceId = deviceId,
        };

        await database.AddAsync(sensor);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ApiSensor?> GetSensor(Guid id, CancellationToken ct = default)
    {
        var dbsModel = await database.Sensors.AsNoTracking().Include(i => i.Metadata).FirstOrDefaultAsync(x => x.Id == id, ct);

        if (dbsModel == null)
            return null;

        return mapper.Map<ApiSensor>(dbsModel);
    }

    public async Task<IEnumerable<ApiSensor>> GetAllSensorsPerDevice(Guid deviceId, CancellationToken ct = default)
    {
        return await database.Sensors.AsNoTracking().Where(x => x.SourceDeviceId == deviceId).ProjectTo<ApiSensor>(mapper.ConfigurationProvider).ToListAsync(ct);
    }

    public async Task<ApiSensor?> GetSensorByActuator(Guid id, CancellationToken ct = default)
    {
        var actuator = await database.Actuators.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (actuator is null)
        {
            return null;
        }

        return await GetSensor(actuator.SensorId, ct);
    }
}
