using Conectify.Database.Models;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.Mapping;
using Conectify.Shared.Library.Models.Values;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Shared.Maps.Test;

public class MapperTests
{
    private readonly IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<EventProfile>()).CreateMapper();

    [Test]
    public void NullSourceGivesDefault()
    {
        Assert.That(mapper.Map<ApiEvent>(null), Is.Null);
    }

    [Test]
    public void MapsSingleObject()
    {
        var id = Guid.NewGuid();

        var result = mapper.Map<ApiEvent>(new Event { Id = id, Name = "temp", NumericValue = 21.5f, TimeCreated = 5 });

        Assert.That(result, Has.Property("Id").EqualTo(id).And.Property("Name").EqualTo("temp").And.Property("NumericValue").EqualTo(21.5f));
    }

    [Test]
    public void MapsCollectionsIntoListAndEnumerable()
    {
        var source = new[] { new Event { Name = "a" }, new Event { Name = "b" } };

        Assert.That(mapper.Map<List<ApiEvent>>(source).Select(x => x.Name), Is.EqualTo(new[] { "a", "b" }));
        Assert.That(mapper.Map<IEnumerable<ApiEvent>>(source).Select(x => x.Name), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void MissingMapThrows()
    {
        Assert.Throws<InvalidOperationException>(() => mapper.Map<Device>(new Event()));
    }

    [Test]
    public void ConfigurationRejectsUnmappedMember()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<WebsocketEvent, ApiEvent>(x => new ApiEvent { Id = x.Id }));

        Assert.Throws<InvalidOperationException>(config.AssertConfigurationIsValid);
    }

    [Test]
    public void AddMappingAccumulatesAssembliesAcrossCalls()
    {
        var services = new ServiceCollection();
        services.AddMapping(typeof(EventProfile).Assembly);
        services.AddMapping(typeof(Conectify.Server.Mapper.SubscriberProfile).Assembly);

        var resolved = services.BuildServiceProvider().GetRequiredService<IMapper>();

        Assert.That(resolved.Map<ApiEvent>(new Event()), Is.Not.Null);
        Assert.That(resolved.Map<Conectify.Server.Caches.Subscriber>(new Device { Sensors = [], Actuators = [] }), Is.Not.Null);
    }
}
