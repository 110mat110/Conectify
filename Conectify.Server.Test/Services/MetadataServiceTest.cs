using AutoMapper;
using Conectify.Database;
using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Conectify.Server.Test.Services;

public class MetadataServiceTest
{
    private DbContextOptions<ConectifyDb> dbContextoptions;
    private IMapper mapper;

    public MetadataServiceTest()
    {
        dbContextoptions = new DbContextOptionsBuilder<ConectifyDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MetadataProfile>();
        }).CreateMapper();
    }

    [Fact]
    public async Task ItShallAddNewMetadataWithId()
    {
        var service = new MetadataService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceStatusService>());
        var id = Guid.NewGuid();
        var apiMetadta = new ApiBasicMetadata()
        {
            Id = id,
            Name = "Name",
        };

        var result = await service.AddNewMetadata(apiMetadta);
        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).Metadatas.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.Equal(id, metadata!.Id);
    }

    [Fact]
    public async Task ItShallAddNewMetadataWithoutId()
    {
        var service = new MetadataService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceStatusService>());
        var apiMetadta = new ApiBasicMetadata()
        {
            Name = "Name",
        };

        var result = await service.AddNewMetadata(apiMetadta);
        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).Metadatas.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.NotEqual(Guid.Empty, metadata!.Id);
    }

    [Fact]
    public async Task ItShallReadAllMetadata()
    {
        var db = new ConectifyDb(dbContextoptions);
        db.Metadatas.Add(new Database.Models.Metadata() { Id = Guid.NewGuid() });
        db.Metadatas.Add(new Database.Models.Metadata() { Id = Guid.NewGuid() });
        db.SaveChanges();

        var service = new MetadataService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceStatusService>());

        var metadatas = await service.GetAllMetadata();
        Assert.Equal(2, metadatas.Count());
    }
}
