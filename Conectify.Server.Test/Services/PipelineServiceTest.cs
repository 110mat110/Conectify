using AutoMapper;
using Conectify.Database;
using Conectify.Database.Interfaces;
using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Conectify.Server.Caches;
using Conectify.Server.Services;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections;
using Action = Conectify.Database.Models.Values.Action;

namespace Conectify.Server.Test.Services
{
    public class PipelineServiceTest
    {
        private DbContextOptions<ConectifyDb> dbContextoptions;
        private IMapper mapper;
        private static Guid sourceDeviceId = Guid.NewGuid();

        public PipelineServiceTest()
        {
            dbContextoptions = new DbContextOptionsBuilder<ConectifyDb>()
                .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ValuesProfile>();
                cfg.AddProfile<PreferenceProfile>();
            }).CreateMapper();
        }

        [Theory]
        [ClassData(typeof(ValueClassData))]
        [ClassData(typeof(ActionClassData))]
        [ClassData(typeof(CommandClassData))]
        [ClassData(typeof(ActionResponseClassData))]
        [ClassData(typeof(CommandResponseClassData))]
        public async Task ItShallNotResendWhenNoSubs(IBaseInputType input)
        {
            var websocketService = A.Fake<IWebSocketService>();
            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), websocketService, mapper, A.Fake<ILogger<PipelineService>>());

            await service.ResendValueToSubscribers(input);
            A.CallTo(() => websocketService.SendToDeviceAsync(A<Guid>.Ignored, A<IWebsocketModel>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ItShallNotFailWhenUnknownModel()
        {
            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.ResendValueToSubscribers(new TestValue());
        }

        [Theory]
        [ClassData(typeof(ValueClassData))]
        [ClassData(typeof(ActionClassData))]
        [ClassData(typeof(CommandClassData))]
        [ClassData(typeof(ActionResponseClassData))]
        [ClassData(typeof(CommandResponseClassData))]
        public async Task ItShallResendToDevicesThatAreSubbedToAll(IBaseInputType input)
        {
            var websocketService = A.Fake<IWebSocketService>();
            var subCahce = A.Fake<ISubscribersCache>();
            var targetDeviceId = Guid.NewGuid();
            A.CallTo(() => subCahce.AllSubscribers()).Returns(new List<Subscriber>() { new Subscriber() { DeviceId = targetDeviceId, IsSubedToAll = true } });
            var service = new PipelineService(new ConectifyDb(dbContextoptions), subCahce, websocketService, mapper, A.Fake<ILogger<PipelineService>>());

            await service.ResendValueToSubscribers(input);
            A.CallTo(() => websocketService.SendToDeviceAsync(targetDeviceId, A<IWebsocketModel>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened();
        }

        [Theory]
        [ClassData(typeof(ValueClassData))]
        [ClassData(typeof(ActionClassData))]
        [ClassData(typeof(CommandClassData))]
        [ClassData(typeof(ActionResponseClassData))]
        [ClassData(typeof(CommandResponseClassData))]
        public async Task ItShallResendToDevicesThatAreSubbedToSource(IBaseInputType input)
        {
            var websocketService = A.Fake<IWebSocketService>();
            var subCahce = A.Fake<ISubscribersCache>();
            var targetDeviceId = Guid.NewGuid();
            A.CallTo(() => subCahce.AllSubscribers())
                .Returns(new List<Subscriber>() {
                    new Subscriber() {
                        DeviceId = targetDeviceId,
                        IsSubedToAll = false,
                        Preferences = new List<Preference>()
                        { new Preference()
                            {
                            SensorId = sourceDeviceId,
                            ActuatorId = sourceDeviceId,
                            DeviceId = sourceDeviceId,
                            SubToActionResponse = true,
                            SubToActions = true,
                            SubToCommandResponse = true,
                            SubToValues = true,
                            SubToCommands = true,
                            }
                        }
                    }});
            var service = new PipelineService(new ConectifyDb(dbContextoptions), subCahce, websocketService, mapper, A.Fake<ILogger<PipelineService>>());

            await service.ResendValueToSubscribers(input);
            A.CallTo(() => websocketService.SendToDeviceAsync(targetDeviceId, A<IWebsocketModel>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened();
        }

        [Theory]
        [ClassData(typeof(ValueClassData))]
        [ClassData(typeof(ActionClassData))]
        [ClassData(typeof(CommandClassData))]
        [ClassData(typeof(ActionResponseClassData))]
        [ClassData(typeof(CommandResponseClassData))]
        public async Task ItShallResendToDevicesThatAreSubbedWithoutSource(IBaseInputType input)
        {
            var websocketService = A.Fake<IWebSocketService>();
            var subCahce = A.Fake<ISubscribersCache>();
            var targetDeviceId = Guid.NewGuid();
            A.CallTo(() => subCahce.AllSubscribers())
                .Returns(new List<Subscriber>() {
                    new Subscriber() {
                        DeviceId = targetDeviceId,
                        IsSubedToAll = false,
                        Preferences = new List<Preference>()
                        { new Preference()
                            {
                            SensorId = null,
                            ActuatorId = null,
                            DeviceId = null,
                            SubToActionResponse = true,
                            SubToActions = true,
                            SubToCommandResponse = true,
                            SubToValues = true,
                            SubToCommands = true,
                            }
                        }
                    }});
            var service = new PipelineService(new ConectifyDb(dbContextoptions), subCahce, websocketService, mapper, A.Fake<ILogger<PipelineService>>());

            await service.ResendValueToSubscribers(input);
            A.CallTo(() => websocketService.SendToDeviceAsync(targetDeviceId, A<IWebsocketModel>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappened();
        }

        [Theory]
        [ClassData(typeof(ValueClassData))]
        [ClassData(typeof(ActionClassData))]
        [ClassData(typeof(CommandClassData))]
        [ClassData(typeof(ActionResponseClassData))]
        [ClassData(typeof(CommandResponseClassData))]
        public async Task ItShallNotResendToDevicesThatAreSubbedButNotToCorrectValue(IBaseInputType input)
        {
            var websocketService = A.Fake<IWebSocketService>();
            var subCahce = A.Fake<ISubscribersCache>();
            var targetDeviceId = Guid.NewGuid();
            A.CallTo(() => subCahce.AllSubscribers())
                .Returns(new List<Subscriber>() {
                    new Subscriber() {
                        DeviceId = targetDeviceId,
                        IsSubedToAll = false,
                        Preferences = new List<Preference>()
                        { new Preference()
                            {
                            SensorId = sourceDeviceId,
                            ActuatorId = sourceDeviceId,
                            DeviceId = sourceDeviceId,
                            SubToActionResponse = false,
                            SubToActions = false,
                            SubToCommandResponse = false,
                            SubToValues = false,
                            SubToCommands = false,
                            }
                        }
                    }});
            var service = new PipelineService(new ConectifyDb(dbContextoptions), subCahce, websocketService, mapper, A.Fake<ILogger<PipelineService>>());

            await service.ResendValueToSubscribers(input);
            A.CallTo(() => websocketService.SendToDeviceAsync(targetDeviceId, A<IWebsocketModel>.Ignored, A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ItShallNotSetPreferenceWhenUnknownDevice()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = false });
            db.SaveChanges();

            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetPreference(sourceDeviceId, new List<ApiPreference>() { new ApiPreference() });

            Assert.Empty(new ConectifyDb(dbContextoptions).Set<Preference>().ToList());
        }
        [Fact]
        public async Task ItShallSubToAllWithoutCallingSubToAll()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = true });
            db.SaveChanges();

            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetPreference(sourceDeviceId, new List<ApiPreference>() { new ApiPreference() { SubToActionResponse = true, SubToActions = true, SubToCommandResponse = true, SubToCommands = true, SubToValues = true } });

            Assert.True(new ConectifyDb(dbContextoptions).Set<Device>().First().SubscribeToAll);
        }

        [Fact]
        public async Task ItShallSubToSpecificSensor()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = true });
            db.SaveChanges();
            var sensorId = Guid.NewGuid();
            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetPreference(sourceDeviceId, new List<ApiPreference>() { new ApiPreference() { SensorId = sensorId } });

            Assert.Equal(sensorId, new ConectifyDb(dbContextoptions).Set<Device>().Include(i => i.Preferences).First().Preferences.First().SensorId);
        }

        [Fact]
        public async Task ItShallUnsubsrcribeFromSensorPreference()
        {
            var sensorId = Guid.NewGuid();
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = true, Preferences = new List<Preference>() { new Preference() { SensorId = sensorId, SubToValues = true } } });
            db.SaveChanges();
            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetPreference(sourceDeviceId, new List<ApiPreference>() { new ApiPreference() { SensorId = sensorId, SubToValues = false } });

            Assert.Equal(sensorId, new ConectifyDb(dbContextoptions).Set<Device>().Include(i => i.Preferences).First().Preferences.First().SensorId);
            Assert.False(new ConectifyDb(dbContextoptions).Set<Device>().Include(i => i.Preferences).First().Preferences.First().SubToValues);
        }

        [Fact]
        public async Task ItShallUpdateSubscibersCache()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = true });
            db.SaveChanges();
            var subCache = A.Fake<ISubscribersCache>();
            var service = new PipelineService(new ConectifyDb(dbContextoptions), subCache, A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetPreference(sourceDeviceId, new List<ApiPreference>() { new ApiPreference() });
            A.CallTo(() => subCache.UpdateSubscriber(sourceDeviceId, A<CancellationToken>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task ItShallNotSubToAllWhenUnknownDevice()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = false });
            db.SaveChanges();

            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetSubscribeToAll(sourceDeviceId, true);

            Assert.False(new ConectifyDb(dbContextoptions).Set<Device>().First().SubscribeToAll);
        }
        [Fact]
        public async Task ItShallSubToAll()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = true, SubscribeToAll = false });
            db.SaveChanges();

            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetSubscribeToAll(sourceDeviceId, true);

            Assert.True(new ConectifyDb(dbContextoptions).Set<Device>().First().SubscribeToAll);
        }
        [Fact]
        public async Task ItShallUnsubAll()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = true, SubscribeToAll = true });
            db.SaveChanges();

            var service = new PipelineService(new ConectifyDb(dbContextoptions), A.Fake<ISubscribersCache>(), A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetSubscribeToAll(sourceDeviceId, false);

            Assert.False(new ConectifyDb(dbContextoptions).Set<Device>().First().SubscribeToAll);
        }
        [Fact]
        public async Task ItShallUpdateSubscibersCacheFromSubToAll()
        {
            var db = new ConectifyDb(dbContextoptions);
            db.Add(new Device() { Id = sourceDeviceId, IsKnown = true });
            db.SaveChanges();
            var subCache = A.Fake<ISubscribersCache>();
            var service = new PipelineService(new ConectifyDb(dbContextoptions), subCache, A.Fake<IWebSocketService>(), mapper, A.Fake<ILogger<PipelineService>>());

            await service.SetSubscribeToAll(sourceDeviceId, true);

            A.CallTo(() => subCache.UpdateSubscriber(sourceDeviceId, A<CancellationToken>.Ignored)).MustHaveHappened();
        }

        public class ValueClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                new Value
                {
                    Id = Guid.NewGuid(),
                    SourceId = sourceDeviceId,
                }
            };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CommandClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                new Command
                {
                    Id = Guid.NewGuid(),
                    SourceId = sourceDeviceId,
                }
            };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class ActionClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                new Action
                {
                    Id = Guid.NewGuid(),
                    SourceId = sourceDeviceId,
                }
            };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class ActionResponseClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                new ActionResponse
                {
                    Id = Guid.NewGuid(),
                    SourceId = sourceDeviceId,
                }
            };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public class CommandResponseClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {
                new CommandResponse
                {
                    Id = Guid.NewGuid(),
                    SourceId = sourceDeviceId,
                }
            };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class TestValue : IBaseInputType
        {
            public Guid SourceId { get; set; }
            public string Name { get; set; }
            public string Unit { get; set; }
            public string StringValue { get; set; }
            public float? NumericValue { get; set; }
            public long TimeCreated { get; set; }
            public Guid Id { get; set; }
        }

    }
}
