namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

public interface ISensorService : IUniversalDeviceService<ApiSensor>
{
    Task<IEnumerable<ApiSensor>> GetAllSensorsPerDevice(Guid deviceId, CancellationToken ct = default);
    Task<ApiSensor?> GetSensorByActuator(Guid id, CancellationToken ct = default);
}

public class SensorService : UniversalDeviceService<Sensor, ApiSensor>, ISensorService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly IDeviceService deviceService;
    private readonly ILogger<SensorService> logger;

    public SensorService(ConectifyDb database, IMapper mapper, IDeviceService deviceService, ILogger<SensorService> logger) : base(database, mapper, logger)
    {
        this.database = database;
        this.mapper = mapper;
        this.deviceService = deviceService;
        this.logger = logger;
    }

    public override async Task<bool> TryAddUnknownDevice(Guid sensorId, Guid deviceId, CancellationToken ct = default)
    {
        if (deviceId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(deviceId));
        }

        if (await database.Sensors.AnyAsync(x => x.Id == sensorId, ct))
        {
            return false;
        }

        logger.LogError($"There is not sensor in dbs with id {sensorId}");
        await deviceService.TryAddUnknownDevice(deviceId, deviceId, ct);

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

        return await GetSpecificDevice(actuator.SensorId, ct);
    }
}
