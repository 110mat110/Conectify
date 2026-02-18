using System.Diagnostics.Metrics;
using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Server.Services;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace Conectify.Server.Test.Services;

public class DataServiceTest
{
    readonly ConectifyDb dbContext;
    public DataServiceTest()
    {
        var contextOptions = new DbContextOptionsBuilder<ConectifyDb>()
            .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        this.dbContext = new ConectifyDb(contextOptions);
    }

    [Fact]
    public async Task ItShallNotCrashWhenInvalidJson() //This is more integration test than UT
    {
        DataService ds = new(A.Fake<ILogger<DataService>>(), dbContext, A.Fake<IPipelineService>(), A.Fake<IDeviceService>(), A.Fake<ISensorService>(), A.Fake<System.Diagnostics.Metrics.IMeterFactory>());

        try
        {
            await ds.InsertJsonModel("!@#$", Guid.NewGuid());
        }
        catch
        {
            Assert.True(false, "Exception was thrown!");
        }
        Assert.True(true);
    }

    [Theory]
    [InlineData("Value")]
    [InlineData("Command")]
    public async void ItShallRegisterIncomingJsonToCorespondingService(string valueType)
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventProfile>();
        }).CreateMapper();

        var sensorService = A.Fake<ISensorService>();
        var deviceService = A.Fake<IDeviceService>();
        var actuatorService = A.Fake<IActuatorService>();
        DataService ds = new(A.Fake<ILogger<DataService>>(), dbContext, A.Fake<IPipelineService>(), deviceService, sensorService, A.Fake<System.Diagnostics.Metrics.IMeterFactory>());

        var sensorId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var validValue = new WebsocketEvent()
        {
            Id = Guid.NewGuid(),
            Name = "testName",
            Type = valueType,
            NumericValue = 10,
            SourceId = sensorId,
            StringValue = "test",
            TimeCreated = 10000,
            Unit = "testUnit"
        };

        await ds.InsertJsonModel(JsonConvert.SerializeObject(validValue), deviceId);

        if (valueType == "Value")
        {
            A.CallTo(() => sensorService.TryAddUnknownDevice(A<Guid>.That.IsEqualTo(sensorId), A<Guid>.That.IsEqualTo(deviceId), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => deviceService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => actuatorService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }
        if (valueType == "Command")
        {
            A.CallTo(() => deviceService.TryAddUnknownDevice(A<Guid>.That.IsEqualTo(sensorId), A<Guid>.That.IsEqualTo(sensorId), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => sensorService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => actuatorService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }
    }

    [Theory]
    [InlineData(typeof(Event))]
    public async void ItShallSaveValueToDatabase(Type valueType)
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventProfile>();
        }).CreateMapper();

        DataService ds = new(A.Fake<ILogger<DataService>>(), dbContext, A.Fake<IPipelineService>(), A.Fake<IDeviceService>(), A.Fake<ISensorService>(), A.Fake<System.Diagnostics.Metrics.IMeterFactory>());

        var id = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var validValue = new WebsocketEvent()
        {
            Id = id,
            Name = "testName",
            Type = valueType.Name,
            NumericValue = 10,
            SourceId = Guid.NewGuid(),
            StringValue = "test",
            TimeCreated = 10000,
            Unit = "testUnit"
        };

        await ds.InsertJsonModel(JsonConvert.SerializeObject(validValue), deviceId);

        var result = dbContext.Find(valueType, id);

        Assert.NotNull(result);
    }
}