using AutoMapper;
using Conectify.Database;
using Conectify.Database.Models.Values;
using Conectify.Server.Services;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Action = Conectify.Database.Models.Values.Action;

namespace Conectify.Server.Test.Services
{
    public class DataServiceTest
    {
        ConectifyDb dbContext;
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
            DataService ds = new(A.Fake<ILogger<DataService>>(), dbContext, A.Fake<IPipelineService>(), A.Fake<IDeviceService>(), A.Fake<ISensorService>(), A.Fake<IActuatorService>(), A.Fake<IMapper>());

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
        [InlineData("Action")]
        [InlineData("ActionResponse")]
        [InlineData("Command")]
        [InlineData("CommandResponse")]
        public async void ItShallRegisterIncomingJsonToCorespondingService(string valueType)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ValuesProfile>();
            }).CreateMapper();

            var sensorService = A.Fake<ISensorService>();
            var deviceService = A.Fake<IDeviceService>();
            var actuatorService = A.Fake<IActuatorService>();
            DataService ds = new(A.Fake<ILogger<DataService>>(), dbContext, A.Fake<IPipelineService>(), deviceService, sensorService, actuatorService, mapper);

            var sensorId = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var validValue = new WebsocketBaseModel()
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

            if(valueType == "Value" || valueType == "Action")
            {
                A.CallTo(() => sensorService.TryAddUnknownDevice(A<Guid>.That.IsEqualTo(sensorId), A<Guid>.That.IsEqualTo(deviceId), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
                A.CallTo(() => deviceService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => actuatorService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            }
            if (valueType == "Command" || valueType == "CommandResponse")
            {
                A.CallTo(() => deviceService.TryAddUnknownDevice(A<Guid>.That.IsEqualTo(sensorId), A<Guid>.That.IsEqualTo(sensorId), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
                A.CallTo(() => sensorService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => actuatorService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            }
            if (valueType == "ActionResponse")
            {
                A.CallTo(() => actuatorService.TryAddUnknownDevice(A<Guid>.That.IsEqualTo(sensorId), A<Guid>.That.IsEqualTo(deviceId), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
                A.CallTo(() => deviceService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
                A.CallTo(() => sensorService.TryAddUnknownDevice(A<Guid>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
            }
        }

        [Theory]
        [InlineData(typeof(Value))]
        [InlineData(typeof(Action))]
        [InlineData(typeof(ActionResponse))]
        [InlineData(typeof(Command))]
        [InlineData(typeof(CommandResponse))]
        public async void ItShallSaveValueToDatabase(Type valueType)
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ValuesProfile>();
            }).CreateMapper();

            DataService ds = new(A.Fake<ILogger<DataService>>(), dbContext, A.Fake<IPipelineService>(), A.Fake<IDeviceService>(), A.Fake<ISensorService>(), A.Fake<IActuatorService>(), mapper);

            var id = Guid.NewGuid();
            var deviceId = Guid.NewGuid();
            var validValue = new WebsocketBaseModel()
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
}