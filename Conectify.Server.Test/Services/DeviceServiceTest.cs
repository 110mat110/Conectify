using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Server.Caches;
using Conectify.Server.Services;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Conectify.Server.Test.Services;

public class DeviceServiceTest
{
    private readonly DbContextOptions<ConectifyDb> dbContextoptions;
    private readonly IMapper mapper;
    private readonly Configuration configuration;

    public DeviceServiceTest()
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
    public async Task ItShallAddUnknownDeviceToDatabase()
    {
        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var deviceId = Guid.NewGuid();

        var result = await service.TryAddUnknownDevice(deviceId);

        Assert.True(result);
        var dbsDevice = new ConectifyDb(dbContextoptions).Devices.FirstOrDefault();
        Assert.NotNull(dbsDevice);
        Assert.Equal(deviceId, dbsDevice!.Id);
        Assert.Equal("unknown device", dbsDevice.Name);
    }

    [Fact]
    public async Task ItShallNotAddDeviceAgainToDatabase()
    {
        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var deviceId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        var deviceName = "Test known device";
        dbs.Add(new Device()
        {
            Id = deviceId,
            Name = deviceName,
        });
        dbs.SaveChanges();

        var result = await service.TryAddUnknownDevice(deviceId);

        Assert.False(result);
        var dbsDevice = new ConectifyDb(dbContextoptions).Devices.FirstOrDefault();
        Assert.NotNull(dbsDevice);
        Assert.Equal(deviceId, dbsDevice!.Id);
        Assert.Equal(deviceName, dbsDevice.Name);
    }

    [Fact]
    public async Task ItShallAddKnownDeviceWithId()
    {
        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var deviceId = Guid.NewGuid();
        var deviceName = Guid.NewGuid().ToString();
        var apiDevice = new ApiDevice()
        {
            IPAdress = "test",
            MacAdress = "test",
            Id = deviceId,
            Name = deviceName,
        };

        var result = await service.AddKnownDevice(apiDevice);

        Assert.Equal(deviceId, result);
        var dbsDevice = new ConectifyDb(dbContextoptions).Devices.FirstOrDefault();
        Assert.NotNull(dbsDevice);
        Assert.Equal(deviceId, dbsDevice!.Id);
        Assert.Equal(deviceName, dbsDevice.Name);
    }

    [Fact]
    public async Task ItShallOverwriteKnownDevice()
    {
        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var deviceName = Guid.NewGuid().ToString();
        var deviceId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Device()
        {
            Id = deviceId,
            Name = "NOT ID",
        });
        dbs.SaveChanges();

        var apiDevice = new ApiDevice()
        {
            IPAdress = "test",
            MacAdress = "test",
            Name = deviceName,
            Id = deviceId,
        };

        var result = await service.AddKnownDevice(apiDevice);

        Assert.Equal(deviceId, result);
        var dbsDevice = new ConectifyDb(dbContextoptions).Devices.FirstOrDefault();
        Assert.NotNull(dbsDevice);
        Assert.Equal(result, dbsDevice!.Id);
        Assert.Equal(deviceName, dbsDevice.Name);
    }

    [Fact]
    public async Task ItShallAddKnownDeviceWithoutId()
    {
        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var deviceName = Guid.NewGuid().ToString();
        var apiDevice = new ApiDevice()
        {
            IPAdress = "test",
            MacAdress = "test",
            Name = deviceName,
        };

        var result = await service.AddKnownDevice(apiDevice);

        var dbsDevice = new ConectifyDb(dbContextoptions).Devices.FirstOrDefault();
        Assert.NotNull(dbsDevice);
        Assert.Equal(result, dbsDevice!.Id);
        Assert.Equal(deviceName, dbsDevice.Name);
    }

    [Fact]
    public async Task ItShallReturnDeviceById()
    {
        var deviceName = Guid.NewGuid().ToString();
        var deviceId = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Device()
        {
            Id = deviceId,
            Name = deviceName,
        });
        dbs.SaveChanges();

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.GetSpecificDevice(deviceId);

        Assert.NotNull(result);
        Assert.Equal(deviceName, result!.Name);
        Assert.Equal(deviceId, result!.Id);
    }

    [Fact]
    public async Task ItShallReturnNullWhenDeviceIsNotInDb()
    {
        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.GetSpecificDevice(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ItShallReturnMultipleDevices()
    {
        var device1Id = Guid.NewGuid();
        var device2Id = Guid.NewGuid();
        var dbs = new ConectifyDb(dbContextoptions);
        dbs.Add(new Device()
        {
            Id = device1Id
        });
        dbs.Add(new Device()
        {
            Id = device2Id
        });
        dbs.SaveChanges();

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.GetAllDevices();

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Id == device1Id);
        Assert.Contains(result, x => x.Id == device2Id);
    }

    [Fact]
    public async Task ItShallNotThrowWhenDeviceNotInDbs()
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

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.AddMetadata(metadataApi);

        Assert.False(result);
    }



    [Fact]
    public async Task ItShallNotThrowWhenMetadataNotInDbs()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var deviceId = Guid.NewGuid();
        dbs.Add(new Device()
        {
            Id = deviceId,
        });
        dbs.SaveChanges();
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = deviceId,
            MetadataId = Guid.NewGuid(),
        };

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.AddMetadata(metadataApi);

        Assert.False(result);
    }

    [Fact]
    public async Task ItShallAddMetadata()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var deviceId = Guid.NewGuid();
        var metadataId = Guid.NewGuid();
        dbs.Add(new Metadata()
        {
            Id = metadataId,
        });
        dbs.Add(new Device()
        {
            Id = deviceId,
        });
        dbs.SaveChanges();
        string stringValue = "test";
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = deviceId,
            MetadataId = metadataId,
            StringValue = stringValue,
        };

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.AddMetadata(metadataApi);

        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).DeviceMetadata.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.Equal(stringValue, metadata!.StringValue);
        Assert.Equal(deviceId, metadata!.DeviceId);
        Assert.Equal(metadataId, metadata!.MetadataId);
    }

    [Fact]
    public async Task ItShallOverwriteExistingMetadataWhenExclusive()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var deviceId = Guid.NewGuid();
        var metadataId = Guid.NewGuid();
        dbs.Add(new Metadata()
        {
            Id = metadataId,
            Exclusive = true,
        });
        dbs.Add(new Device()
        {
            Id = deviceId,
        });
        dbs.Add(new MetadataConnector<Device>()
        {
            DeviceId = deviceId,
            MetadataId = metadataId,
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.SaveChanges();
        string stringValue = "test";
        var metadataApi = new ApiMetadataConnector()
        {
            DeviceId = deviceId,
            MetadataId = metadataId,
            StringValue = stringValue,
        };

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.AddMetadata(metadataApi);

        Assert.True(result);
        var metadata = new ConectifyDb(dbContextoptions).DeviceMetadata.FirstOrDefault();
        Assert.NotNull(metadata);
        Assert.Equal(stringValue, metadata!.StringValue);
        Assert.Equal(deviceId, metadata!.DeviceId);
        Assert.Equal(metadataId, metadata!.MetadataId);

        var allMetadatas = new ConectifyDb(dbContextoptions).DeviceMetadata.ToList();
        Assert.Single(allMetadatas);
    }

    [Fact]
    public async Task ItShallReturnMultipleMetadata()
    {
        var dbs = new ConectifyDb(dbContextoptions);
        var deviceId = Guid.NewGuid();
        dbs.Add(new Device()
        {
            Id = deviceId,
        });
        dbs.Add(new MetadataConnector<Device>()
        {
            DeviceId = deviceId,
            Metadata = new Metadata(),
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.Add(new MetadataConnector<Device>()
        {
            DeviceId = deviceId,
            Metadata = new Metadata(),
            StringValue = Guid.NewGuid().ToString(),
        });
        dbs.SaveChanges();

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var result = await service.GetMetadata(deviceId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ItShallFilterByVisibility()
    {
        var dbs = new ConectifyDb(dbContextoptions);

        var visibleDeviceId = Guid.NewGuid();
        var hiddenDeviceId = Guid.NewGuid();
        dbs.Add(new Device()
        {
            Id = hiddenDeviceId,
        });
        dbs.Add(new MetadataConnector<Device>()
        {
            DeviceId = hiddenDeviceId,
            Metadata = new Metadata() { Name = Constants.Metadatas.Visible },
            NumericValue = 0,
        });
        dbs.Add(new Device()
        {
            Id = visibleDeviceId,
        });
        dbs.Add(new MetadataConnector<Device>()
        {
            DeviceId = visibleDeviceId,
            Metadata = new Metadata() { Name = Constants.Metadatas.Visible },
            NumericValue = 1,
        });
        dbs.SaveChanges();

        var service = new DeviceService(new ConectifyDb(dbContextoptions), mapper, A.Fake<ILogger<DeviceService>>(), A.Fake<IHttpFactory>(), configuration, A.Fake<IWebsocketCache>());
        var filter = new ApiFilter() { IsVisible = true };
        var result = (await service.Filter(filter)).Select(x => x.Id);

        Assert.Contains<Guid>(visibleDeviceId, result);
        Assert.DoesNotContain<Guid>(hiddenDeviceId, result);
    }
}
