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
}
