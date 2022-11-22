using AutoMapper;
using Conectify.Server.Mapper;
using Conectify.Services.Automatization.Mapper;
using System.Reflection;

namespace Conectify.Shared.Maps.Test;

public class Tests
{

    [Test]
    public void MapperConfigTest()
    {
        var config = new MapperConfiguration(configuration =>
        {
            configuration.AddMaps(typeof(ValuesProfile).GetTypeInfo().Assembly);
            configuration.AddMaps(typeof(SubscriberProfile).GetTypeInfo().Assembly);
            configuration.AddMaps(typeof(RuleProfile).GetTypeInfo().Assembly);
        });

        // Assert
        config.AssertConfigurationIsValid();
    }
}