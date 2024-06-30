using AutoMapper;
using AutoMapper.QueryableExtensions;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Shared.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Conectify.Server.Services;

public interface IMetadataService
{
    public Task<IEnumerable<ApiBasicMetadata>> GetAllMetadata(CancellationToken ct = default);

    public Task<bool> AddNewMetadata(ApiBasicMetadata metadata, CancellationToken ct = default);

    public Task<ApiBasicMetadata?> GetMetadataByCode(string code, CancellationToken ct = default);

    public Task<bool> Remove(Guid metadataId, Guid deviceId, CancellationToken ct = default);
    public Task<bool> Remove(Guid id, CancellationToken ct = default);

}

public class MetadataService : IMetadataService
{
    private readonly ConectifyDb database;
    private readonly IMapper mapper;

    public MetadataService(ConectifyDb database, IMapper mapper, IDeviceStatusService deviceStatusService)
    {
        this.database = database;
        this.mapper = mapper;
        deviceStatusService.CheckIfAlive();
    }
    public async Task<bool> AddNewMetadata(ApiBasicMetadata metadata, CancellationToken ct = default)
    {
        if (metadata.Id == Guid.Empty)
        {
            metadata.Id = Guid.NewGuid();
        }

        var dbModel = mapper.Map<Metadata>(metadata);
        await database.Set<Metadata>().AddAsync(dbModel, ct);

        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<ApiBasicMetadata>> GetAllMetadata(CancellationToken ct = default)
    {
        return await database.Set<Metadata>().ProjectTo<ApiBasicMetadata>(mapper.ConfigurationProvider).ToListAsync(ct);
    }

	public async Task<ApiBasicMetadata?> GetMetadataByCode(string code, CancellationToken ct = default)
	{
		var metadata = await database.Set<Metadata>().FirstOrDefaultAsync(metadata => metadata.Code.ToLower() == code.ToLower(), ct);
        return mapper.Map<ApiBasicMetadata>(metadata);
	}

    public async Task<bool> Remove(Guid metadataId, Guid deviceId, CancellationToken ct = default)
    {
        var deviceMetadata = await database.Set<MetadataConnector<Device>>().FirstOrDefaultAsync(x => x.MetadataId == metadataId && x.DeviceId == deviceId, ct);

        if(deviceMetadata != null)
        {
            database.Set<MetadataConnector<Device>>().Remove(deviceMetadata);
            return true;
        }

        var actuatorMetadata = await database.Set<MetadataConnector<Actuator>>().FirstOrDefaultAsync(x => x.MetadataId == metadataId && x.DeviceId == deviceId, ct);

        if (actuatorMetadata != null)
        {
            database.Set<MetadataConnector<Actuator>>().Remove(actuatorMetadata);
            return true;
        }

        var sensorMetadata = await database.Set<MetadataConnector<Sensor>>().FirstOrDefaultAsync(x => x.MetadataId == metadataId && x.DeviceId == deviceId, ct);

        if (sensorMetadata != null)
        {
            database.Set<MetadataConnector<Sensor>>().Remove(sensorMetadata);
            return true;
        }

        return false;
    }

    public async Task<bool> Remove(Guid id, CancellationToken ct = default)
    {
        var deviceMetadata = await database.Set<MetadataConnector<Device>>().FirstOrDefaultAsync(x => x.Id == id, ct);

        if (deviceMetadata != null)
        {
            database.Set<MetadataConnector<Device>>().Remove(deviceMetadata);
            return true;
        }

        var actuatorMetadata = await database.Set<MetadataConnector<Actuator>>().FirstOrDefaultAsync(x => x.Id == id, ct);

        if (actuatorMetadata != null)
        {
            database.Set<MetadataConnector<Actuator>>().Remove(actuatorMetadata);
            return true;
        }

        var sensorMetadata = await database.Set<MetadataConnector<Sensor>>().FirstOrDefaultAsync(x => x.Id == id, ct);

        if (sensorMetadata != null)
        {
            database.Set<MetadataConnector<Sensor>>().Remove(sensorMetadata);
            return true;
        }

        return false;
    }
}
