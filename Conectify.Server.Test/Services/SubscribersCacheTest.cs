using AutoMapper;
using Castle.Core.Internal;
using Conectify.Database;
using Conectify.Database.Models;
using Conectify.Server.Caches;
using Conectify.Server.Mapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conectify.Server.Test.Services
{
    public class SubscribersCacheTest
    {
        private ConectifyDb dbContext;
        private IServiceProvider serviceProvider;

        public SubscribersCacheTest()
        {
            var contextOptions = new DbContextOptionsBuilder<ConectifyDb>()
                .UseInMemoryDatabase(databaseName: "Test-" + Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            this.dbContext = new ConectifyDb(contextOptions);

            var services = new ServiceCollection();
            services.AddTransient<ConectifyDb>(services => new ConectifyDb(contextOptions));
            serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void ItShallBeCreatedEmpty()
        {
            var subsCache = new SubscribersCache(A.Fake<IServiceProvider>(), A.Fake<IMapper>());
            
            var allSubs = subsCache.AllSubscribers();

            Assert.Empty(allSubs);
        }

        [Fact]
        public void ItShallNotFailWhenGettingNonExisting()
        {
            var subsCache = new SubscribersCache(A.Fake<IServiceProvider>(), A.Fake<IMapper>());

            var sub = subsCache.GetSubscriber(Guid.NewGuid());

            Assert.Null(sub);
        }

        [Fact]
        public void ItShallNotFailWhenRemovingNonExisting()
        {
            var subsCache = new SubscribersCache(A.Fake<IServiceProvider>(), A.Fake<IMapper>());

            var result = subsCache.RemoveSubscriber(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task ItShallUpdateSameSubMultipleTimes()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SubscriberProfile>();
            }).CreateMapper();

            var deviceId = Guid.NewGuid();
            var device = new Device()
            {
                Id = deviceId,
                Name = "test",
                MacAdress = "test",
                IPAdress = "test",
                Preferences = new List<Preference>()
                {
                    new Preference()
                    {
                        SubscriberId=deviceId,
                        Id = Guid.NewGuid(),
                        SubToValues = true,
                    }
                }
            };
            await this.dbContext.AddAsync(device);
            await this.dbContext.SaveChangesAsync();

            var subsCache = new SubscribersCache(serviceProvider, mapper);

            var result1 = await subsCache.UpdateSubscriber(deviceId);
            var result2 = await subsCache.UpdateSubscriber(deviceId);

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(deviceId, result1!.DeviceId);
            Assert.Equal(deviceId, result2!.DeviceId);
        }

        [Fact]
        public async Task ItShallReturnExistingSubscriber()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SubscriberProfile>();
            }).CreateMapper();

            var deviceId = Guid.NewGuid();
            var device = new Device()
            {
                Id = deviceId,
                Name = "test",
                MacAdress = "test",
                IPAdress = "test",
                Preferences = new List<Preference>()
                {
                    new Preference()
                    {
                        SubscriberId=deviceId,
                        Id = Guid.NewGuid(),
                        SubToValues = true,
                    }
                }
            };
            await this.dbContext.AddAsync(device);
            await this.dbContext.SaveChangesAsync();

            var subsCache = new SubscribersCache(serviceProvider, mapper);

            var emptyResult = subsCache.GetSubscriber(deviceId);
            var updateResult = await subsCache.UpdateSubscriber(deviceId);
            var cachedResult = subsCache.GetSubscriber(deviceId);

            Assert.Null(emptyResult);
            Assert.NotNull(updateResult);
            Assert.NotNull(cachedResult);
            Assert.Equal(deviceId, updateResult!.DeviceId);
            Assert.Equal(deviceId, cachedResult!.DeviceId);
        }

        [Fact]
        public async Task ItShallNotFallWhenNoneExistingSub()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SubscriberProfile>();
            }).CreateMapper();

            var deviceId = Guid.NewGuid();

            var subsCache = new SubscribersCache(serviceProvider, mapper);

            var result = await subsCache.UpdateSubscriber(deviceId);

            Assert.Null(result);
        }
    }
}
