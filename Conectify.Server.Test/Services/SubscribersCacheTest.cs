using AutoMapper;
using Conectify.Server.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conectify.Server.Test.Services
{
    public class SubscribersCacheTest
    {
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
        public void ItShallReturnExistingSubscriber()
        {
            
            var subsCache = new SubscribersCache(A.Fake<IServiceProvider>(), A.Fake<IMapper>());

            var result = subsCache.RemoveSubscriber(Guid.NewGuid());

            Assert.False(result);
        }
    }
}
