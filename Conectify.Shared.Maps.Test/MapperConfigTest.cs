using System.Reflection;
using Conectify.Shared.Library.Mapping;
using Conectify.Server.Mapper;
using Conectify.Services.Automatization.Mapper;

namespace Conectify.Shared.Maps.Test;

public class Tests
{

    [Test]
    public void MapperConfigTest()
    {
        var config = new MapperConfiguration(configuration =>
        {
            configuration.AddProfiles(typeof(EventProfile).GetTypeInfo().Assembly);
            configuration.AddProfiles(typeof(SubscriberProfile).GetTypeInfo().Assembly);
            configuration.AddProfiles(typeof(RuleProfile).GetTypeInfo().Assembly);
            configuration.AddProfiles(typeof(Conectify.Services.Dashboard.Mapper.DashboardProfile).GetTypeInfo().Assembly);
        });

        // Assert
        config.AssertConfigurationIsValid();
    }
}
