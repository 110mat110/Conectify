namespace Conectify.Server.Services;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Services;
using Microsoft.EntityFrameworkCore;

public interface ISensorService : IUniversalDeviceService<ApiSensor>
{
    Task<IEnumerable<ApiSensor>> GetAllSensorsPerDevice(Guid deviceId, CancellationToken ct = default);
    Task<ApiSensor?> GetSensorByActuator(Guid id, CancellationToken ct = default);
    Task<ApiValue?> GetLastValue(Guid sensorId, CancellationToken ct = default);
}

public class SensorService : UniversalDeviceService<Sensor, ApiSensor>, ISensorService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;
    private readonly IDeviceService deviceService;
    private readonly ILogger<SensorService> logger;

    public SensorService(ConectifyDb database, IMapper mapper, IDeviceService deviceService, ILogger<SensorService> logger, IHttpFactory httpFactory, Configuration configuration) : base(database, mapper, logger, httpFactory, configuration)
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

    public override async Task<IEnumerable<ApiMetadata>> GetMetadata(Guid sensorId, CancellationToken ct = default)
    {
        var sensor = await database.Set<Sensor>().FirstOrDefaultAsync(x => x.Id == sensorId, ct);
        if (sensor is null)
        {
            return new List<ApiMetadata>();
        }
        var sensorMetadatas = await database.Set<MetadataConnector<Sensor>>().Where(x => x.DeviceId == sensorId).AsNoTracking().ProjectTo<ApiMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);

        var deviceMetadata = await database.Set<MetadataConnector<Device>>().Where(x => x.DeviceId == sensor.SourceDeviceId).AsNoTracking().ProjectTo<ApiMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);

        return sensorMetadatas.Concat(deviceMetadata.Where(x => !sensorMetadatas.Select(x => x.Name).Contains(x.Name)));
    }

    public override async Task<IEnumerable<ApiSensor>> Filter(ApiFilter filter, CancellationToken ct = default)
    {
        var exclude = new List<Guid>();
        if (filter.IsVisible)
        {
            exclude.AddRange(await database.Set<MetadataConnector<Device>>().AsNoTracking().Include(x => x.Metadata).Where(x => x.Metadata.Name == Constants.Metadatas.Visible && x.NumericValue == 0).Select(x => x.DeviceId).ToListAsync(ct));
        }

        if (!string.IsNullOrEmpty(filter.Name))
        {
            return await database.Set<Sensor>().AsNoTracking().Where(x => x.Name.Contains(filter.Name) && !exclude.Contains(x.Id)).ProjectTo<ApiSensor>(mapper.ConfigurationProvider).ToListAsync(ct);
        }

        return await database.Set<Sensor>().AsNoTracking().Where(x => !exclude.Contains(x.Id)).ProjectTo<ApiSensor>(mapper.ConfigurationProvider).ToListAsync(ct);
    }

    public async Task<ApiValue?> GetLastValue(Guid sensorId, CancellationToken ct = default)
    {
        return await database.Set<Value>().AsNoTracking().Where(x => x.SourceId == sensorId).OrderByDescending(x => x.TimeCreated).ProjectTo<ApiValue>(mapper.ConfigurationProvider).FirstOrDefaultAsync(ct);
    }
}
