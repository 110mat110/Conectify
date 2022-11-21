using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Conectify.Server.Test.Services;

public class ActuatorServiceTest
{
    private DbContextOptions<ConectifyDb> dbContextoptions;
    private IMapper mapper;

    public ActuatorServiceTest()
    {
        dbContextoptions = new DbContextOptionsBuilder<ConectifyDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DeviceProfile>();
            cfg.AddProfile<MetadataProfile>();
        }).CreateMapper();
    }

    [Fact]
    public async Task ItShallAddUnknownActuatorToDatabase()
    {
        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var actuatorId = Guid.NewGuid();

        var result = await service.TryAddUnknownDevice(actuatorId, Guid.NewGuid());

        Assert.True(result);
        var dbsActuator = new ConectifyDb(dbContextoptions).Actuators.FirstOrDefault();
        Assert.NotNull(dbsActuator);
        Assert.Equal(actuatorId, dbsActuator!.Id);
        Assert.Equal("unknown Actuator", dbsActuator.Name);
    }

    [Fact]
    public async Task ItShallNotAddActuatorAgainToDatabase()
    {
        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var actuatorId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        var ActuatorName = "Test known Actuator";
        dbs.Add(new Actuator()
        {
            Id = actuatorId,
            Name = ActuatorName,
        });
        dbs.SaveChanges();

        var result = await service.TryAddUnknownDevice(actuatorId, Guid.NewGuid());

        Assert.False(result);
        var dbsActuator = new ConectifyDb(dbContextoptions).Actuators.FirstOrDefault();
        Assert.NotNull(dbsActuator);
        Assert.Equal(actuatorId, dbsActuator!.Id);
        Assert.Equal(ActuatorName, dbsActuator.Name);
    }

    [Fact]
    public async Task ItShallAddKnownDeviceWithId()
    {
        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var actuatorId = Guid.NewGuid();
        var ActuatorName = Guid.NewGuid().ToString();
        var apiActuator = new ApiActuator()
        {
            Id = actuatorId,
            Name = ActuatorName,
        };

        var result = await service.AddKnownDevice(apiActuator);

        Assert.Equal(actuatorId, result);
        var dbsActuator = new ConectifyDb(dbContextoptions).Actuators.FirstOrDefault();
        Assert.NotNull(dbsActuator);
        Assert.Equal(actuatorId, dbsActuator!.Id);
        Assert.Equal(ActuatorName, dbsActuator.Name);
    }

    [Fact]
    public async Task ItShallOverwriteKnownActuator()
    {
        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var ActuatorName = Guid.NewGuid().ToString();
        var actuatorId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Actuator()
        {
            Id = actuatorId,
            Name = "NOT ID",
        });
        dbs.SaveChanges();

        var apiActuator = new ApiActuator()
        {
            Name = ActuatorName,
            Id = actuatorId,
        };

        var result = await service.AddKnownDevice(apiActuator);

        Assert.Equal(actuatorId, result);
        var dbsActuator = new ConectifyDb(dbContextoptions).Actuators.FirstOrDefault();
        Assert.NotNull(dbsActuator);
        Assert.Equal(result, dbsActuator!.Id);
        Assert.Equal(ActuatorName, dbsActuator.Name);
    }

    [Fact]
    public async Task ItShallAddKnownDeviceWithoutId()
    {
        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var ActuatorName = Guid.NewGuid().ToString();
        var apiActuator = new ApiActuator()
        {
            Name = ActuatorName,
        };

        var result = await service.AddKnownDevice(apiActuator);

        var dbsActuator = new ConectifyDb(dbContextoptions).Actuators.FirstOrDefault();
        Assert.NotNull(dbsActuator);
        Assert.Equal(result, dbsActuator!.Id);
        Assert.Equal(ActuatorName, dbsActuator.Name);
    }

    [Fact]
    public async Task ItShallReturnActuatorById()
    {
        var ActuatorName = Guid.NewGuid().ToString();
        var actuatorId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Actuator()
        {
            Id = actuatorId,
            Name = ActuatorName,
        });
        dbs.SaveChanges();

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.GetSpecificDevice(actuatorId);

        Assert.NotNull(result);
        Assert.Equal(ActuatorName, result!.Name);
        Assert.Equal(actuatorId, result!.Id);
    }

    [Fact]
    public async Task ItShallReturnNullWhenActuatorIsNotInDb()
    {
        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.GetSpecificDevice(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ItShallReturnMultipleActuators()
    {
        var Actuator1Id = Guid.NewGuid();
        var Actuator2Id = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Actuator()
        {
            Id = Actuator1Id,
        });
        dbs.Add(new Actuator()
        {
            Id = Actuator2Id,
        });
        dbs.SaveChanges();

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.GetAllDevices();

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Id == Actuator1Id);
        Assert.Contains(result, x => x.Id == Actuator2Id);
    }

    [Fact]
    public async Task ItShallNotThrowWhenActuatorNotInDbs()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var metadataId = Guid.NewGuid();
        dbs.Add(new Metadata()
        {
            Id = metadataId,
        });
        dbs.SaveChanges();
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = Guid.NewGuid(),
            MetadataId = metadataId,
        };

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.AddMetadata(metadataApi);

        Assert.False(result);
    }



    [Fact]
    public async Task ItShallNotThrowWhenMetadataNotInDbs()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var actuatorId = Guid.NewGuid();
        dbs.Add(new Actuator()
        {
            Id = actuatorId,
        });
        dbs.SaveChanges();
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = actuatorId,
            MetadataId = Guid.NewGuid(),
        };

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.AddMetadata(metadataApi);

        Assert.False(result);
    }

    [Fact]
    public async Task ItShallAddMetadata()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var actuatorId = Guid.NewGuid();
        var metadataId = Guid.NewGuid();
        dbs.Add(new Metadata()
        {
            Id = metadataId,
        });
        dbs.Add(new Actuator()
        {
            Id = actuatorId,
        });
        dbs.SaveChanges();
        string stringValue = "test";
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = actuatorId,
            MetadataId = metadataId,
            StringValue = stringValue,
        };

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.AddMetadata(metadataApi);

        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).ActuatorMetadatas.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.Equal(stringValue, metadata!.StringValue);
        Assert.Equal(actuatorId, metadata!.DeviceId);
        Assert.Equal(metadataId, metadata!.MetadataId);
    }

    [Fact]
    public async Task ItShallOverwriteExistingMetadata()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var actuatorId = Guid.NewGuid();
        var metadataId = Guid.NewGuid();
        dbs.Add(new Metadata()
        {
            Id = metadataId,
        });
        dbs.Add(new Actuator()
        {
            Id = actuatorId,
        });
        dbs.Add(new MetadataConnector<Actuator>()
        {
            DeviceId = actuatorId,
            MetadataId = metadataId,
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.SaveChanges();
        string stringValue = "test";
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = actuatorId,
            MetadataId = metadataId,
            StringValue = stringValue,
        };

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.AddMetadata(metadataApi);

        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).ActuatorMetadatas.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.Equal(stringValue, metadata!.StringValue);
        Assert.Equal(actuatorId, metadata!.DeviceId);
        Assert.Equal(metadataId, metadata!.MetadataId);

        var allMetadatas = new ConectifyDb(dbContextoptions).ActuatorMetadatas.ToList();
        Assert.Single(allMetadatas);
    }

    [Fact]
    public async Task ItShallReturnMultipleMetadata()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var actuatorId = Guid.NewGuid();
        dbs.Add(new Actuator()
        {
            Id = actuatorId,
        });
        dbs.Add(new MetadataConnector<Actuator>()
        {
            DeviceId = actuatorId,
            Metadata = new Metadata(),
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.Add(new MetadataConnector<Actuator>()
        {
            DeviceId = actuatorId,
            Metadata = new Metadata(),
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.SaveChanges();

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.GetMetadata(actuatorId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ItShallFailWhenNoDeviceProvided()
    {
        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        try
        {
            await service.TryAddUnknownDevice(Guid.NewGuid(), Guid.Empty);
        }
        catch (ArgumentNullException)
        {
            Assert.True(true);
        }
    }

    [Fact]
    public async Task ItShallGetAllActuatorsOfDevice()
    {
        var deviceId = Guid.NewGuid();
        var db = new ConectifyDb(dbContextoptions);
        db.Add(new Actuator() { SourceDeviceId = deviceId });
        db.Add(new Actuator() { SourceDeviceId = deviceId });
        db.SaveChanges();

        var service = new ActuatorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<ActuatorService>>());
        var result = await service.GetAllActuatorsPerDevice(deviceId);

        Assert.Equal(2, result.Count());
    }
}
