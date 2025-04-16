using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Server.Services;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Conectify.Server.Test.Services;

public class SensorServiceTest
{
    private readonly DbContextOptions<ConectifyDb> dbContextoptions;
    private readonly IMapper mapper;
    private readonly Configuration configuration;

    public SensorServiceTest()
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

        configuration = A.Fake<Configuration>();
    }

    [Fact]
    public async Task ItShallAddUnknownSensorToDatabase()
    {
        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var SensorId = Guid.NewGuid();

        var result = await service.TryAddUnknownDevice(SensorId, Guid.NewGuid());

        Assert.True(result);
        var dbsSensor = new ConectifyDb(dbContextoptions).Sensors.FirstOrDefault();
        Assert.NotNull(dbsSensor);
        Assert.Equal(SensorId, dbsSensor!.Id);
        Assert.Equal("unknown sensor", dbsSensor.Name);
    }

    [Fact]
    public async Task ItShallNotAddSensorAgainToDatabase()
    {
        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var SensorId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        var SensorName = "Test known Sensor";
        dbs.Add(new Sensor()
        {
            Id = SensorId,
            Name = SensorName,
        });
        dbs.SaveChanges();

        var result = await service.TryAddUnknownDevice(SensorId, Guid.NewGuid());

        Assert.False(result);
        var dbsSensor = new ConectifyDb(dbContextoptions).Sensors.FirstOrDefault();
        Assert.NotNull(dbsSensor);
        Assert.Equal(SensorId, dbsSensor!.Id);
        Assert.Equal(SensorName, dbsSensor.Name);
    }

    [Fact]
    public async Task ItShallAddKnownDeviceWithId()
    {
        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var SensorId = Guid.NewGuid();
        var SensorName = Guid.NewGuid().ToString();
        var apiSensor = new ApiSensor()
        {
            Id = SensorId,
            Name = SensorName,
        };

        var result = await service.AddKnownDevice(apiSensor);

        Assert.Equal(SensorId, result);
        var dbsSensor = new ConectifyDb(dbContextoptions).Sensors.FirstOrDefault();
        Assert.NotNull(dbsSensor);
        Assert.Equal(SensorId, dbsSensor!.Id);
        Assert.Equal(SensorName, dbsSensor.Name);
    }

    [Fact]
    public async Task ItShallOverwriteKnownSensor()
    {
        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var SensorName = Guid.NewGuid().ToString();
        var SensorId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Sensor()
        {
            Id = SensorId,
            Name = "NOT ID",
        });
        dbs.SaveChanges();

        var apiSensor = new ApiSensor()
        {
            Name = SensorName,
            Id = SensorId,
        };

        var result = await service.AddKnownDevice(apiSensor);

        Assert.Equal(SensorId, result);
        var dbsSensor = new ConectifyDb(dbContextoptions).Sensors.FirstOrDefault();
        Assert.NotNull(dbsSensor);
        Assert.Equal(result, dbsSensor!.Id);
        Assert.Equal(SensorName, dbsSensor.Name);
    }

    [Fact]
    public async Task ItShallAddKnownDeviceWithoutId()
    {
        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var SensorName = Guid.NewGuid().ToString();
        var apiSensor = new ApiSensor()
        {
            Name = SensorName,
        };

        var result = await service.AddKnownDevice(apiSensor);

        var dbsSensor = new ConectifyDb(dbContextoptions).Sensors.FirstOrDefault();
        Assert.NotNull(dbsSensor);
        Assert.Equal(result, dbsSensor!.Id);
        Assert.Equal(SensorName, dbsSensor.Name);
    }

    [Fact]
    public async Task ItShallReturnSensorById()
    {
        var SensorName = Guid.NewGuid().ToString();
        var SensorId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Sensor()
        {
            Id = SensorId,
            Name = SensorName,
        });
        dbs.SaveChanges();

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.GetSpecificDevice(SensorId);

        Assert.NotNull(result);
        Assert.Equal(SensorName, result!.Name);
        Assert.Equal(SensorId, result!.Id);
    }

    [Fact]
    public async Task ItShallReturnNullWhenSensorIsNotInDb()
    {
        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.GetSpecificDevice(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ItShallReturnMultipleSensors()
    {
        var Sensor1Id = Guid.NewGuid();
        var Sensor2Id = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Sensor()
        {
            Id = Sensor1Id,
        });
        dbs.Add(new Sensor()
        {
            Id = Sensor2Id,
        });
        dbs.SaveChanges();

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.GetAllDevices();

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Id == Sensor1Id);
        Assert.Contains(result, x => x.Id == Sensor2Id);
    }

    [Fact]
    public async Task ItShallNotThrowWhenSensorNotInDbs()
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

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.AddMetadata(metadataApi);

        Assert.False(result);
    }



    [Fact]
    public async Task ItShallNotThrowWhenMetadataNotInDbs()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var SensorId = Guid.NewGuid();
        dbs.Add(new Sensor()
        {
            Id = SensorId,
        });
        dbs.SaveChanges();
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = SensorId,
            MetadataId = Guid.NewGuid(),
        };

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.AddMetadata(metadataApi);

        Assert.False(result);
    }

    [Fact]
    public async Task ItShallAddMetadata()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var SensorId = Guid.NewGuid();
        var metadataId = Guid.NewGuid();
        dbs.Add(new Metadata()
        {
            Id = metadataId,
        });
        dbs.Add(new Sensor()
        {
            Id = SensorId,
        });
        dbs.SaveChanges();
        string stringValue = "test";
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = SensorId,
            MetadataId = metadataId,
            StringValue = stringValue,
        };

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.AddMetadata(metadataApi);

        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).SensorMetadata.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.Equal(stringValue, metadata!.StringValue);
        Assert.Equal(SensorId, metadata!.DeviceId);
        Assert.Equal(metadataId, metadata!.MetadataId);
    }

    [Fact]
    public async Task ItShallOverwriteExistingMetadataWhenExclusive()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var SensorId = Guid.NewGuid();
        var metadataId = Guid.NewGuid();
        dbs.Add(new Metadata()
        {
            Id = metadataId,
            Exclusive = true,
        });
        dbs.Add(new Sensor()
        {
            Id = SensorId,
        });
        dbs.Add(new MetadataConnector<Sensor>()
        {
            DeviceId = SensorId,
            MetadataId = metadataId,
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.SaveChanges();
        string stringValue = "test";
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = SensorId,
            MetadataId = metadataId,
            StringValue = stringValue,
        };

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.AddMetadata(metadataApi);

        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).SensorMetadata.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.Equal(stringValue, metadata!.StringValue);
        Assert.Equal(SensorId, metadata!.DeviceId);
        Assert.Equal(metadataId, metadata!.MetadataId);

        var allMetadatas = new ConectifyDb(dbContextoptions).SensorMetadata.ToList();
        Assert.Single(allMetadatas);
    }

    [Fact]
    public async Task ItShallReturnMultipleMetadata()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var SensorId = Guid.NewGuid();
        dbs.Add(new Sensor()
        {
            Id = SensorId,
        });
        dbs.Add(new MetadataConnector<Sensor>()
        {
            DeviceId = SensorId,
            Metadata = new Metadata(),
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.Add(new MetadataConnector<Sensor>()
        {
            DeviceId = SensorId,
            Metadata = new Metadata(),
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.SaveChanges();

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.GetMetadata(SensorId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ItShallFailWhenNoDeviceProvided()
    {
        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
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
    public async Task ItShallGetAllSensorsOfDevice()
    {
        var deviceId = Guid.NewGuid();
        var db = new ConectifyDb(dbContextoptions);
        db.Add(new Sensor() { SourceDeviceId = deviceId });
        db.Add(new Sensor() { SourceDeviceId = deviceId });
        db.SaveChanges();

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.GetAllSensorsPerDevice(deviceId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ItShallGetSensorByActuatorId()
    {
        var sensorId = Guid.NewGuid();
        var actuatorId = Guid.NewGuid();
        var db = new ConectifyDb(dbContextoptions);
        db.Add(new Actuator() { Id = actuatorId, SensorId = sensorId });
        db.Add(new Sensor() { Id = sensorId });
        db.SaveChanges();

        var service = new SensorService(new ConectifyDb(dbContextoptions), mapper, A.Fake<IDeviceService>(), A.Fake<ILogger<SensorService>>(), A.Fake<IHttpFactory>(), configuration);
        var result = await service.GetSensorByActuator(actuatorId);

        Assert.Equal(sensorId, result!.Id);
    }
}
