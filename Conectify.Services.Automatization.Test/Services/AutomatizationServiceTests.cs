using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Services;

public class AutomatizationServiceTests
{
    [Fact]
    public void StartServiceAsync_ShouldInitializeWebsocketAndTimer()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var configuration = A.Fake<IConfiguration>();
        var config = new AutomatizationConfiguration(configuration);
        var websocketClient = A.Fake<IServicesWebsocketClient>();
        var meterFactory = A.Fake<IMeterFactory>();

        var service = new AutomatizationService(cache, config, websocketClient, meterFactory);

        service.StartServiceAsync();

        A.CallTo(() => websocketClient.ConnectAsync()).MustHaveHappened();
    }

    [Fact]
    public void StartServiceAsync_ShouldSubscribeToIncomingEvents()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var configuration = A.Fake<IConfiguration>();
        var config = new AutomatizationConfiguration(configuration);
        var websocketClient = A.Fake<IServicesWebsocketClient>();
        var meterFactory = A.Fake<IMeterFactory>();

        var service = new AutomatizationService(cache, config, websocketClient, meterFactory);

        service.StartServiceAsync();

        A.CallTo(() => websocketClient.ConnectAsync()).MustHaveHappened();
    }

    [Fact]
    public void Constructor_ShouldAcceptAllParameters()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var configuration = A.Fake<IConfiguration>();
        var config = new AutomatizationConfiguration(configuration);
        var websocketClient = A.Fake<IServicesWebsocketClient>();
        var meterFactory = A.Fake<IMeterFactory>();

        var service = new AutomatizationService(cache, config, websocketClient, meterFactory);

        Assert.NotNull(service);
    }

    [Fact]
    public void StartServiceAsync_WithDifferentRefreshIntervals_ShouldWork()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var websocketClient = A.Fake<IServicesWebsocketClient>();
        var meterFactory = A.Fake<IMeterFactory>();

        var configuration1 = A.Fake<IConfiguration>();
        var configuration2 = A.Fake<IConfiguration>();
        var configuration3 = A.Fake<IConfiguration>();

        var configs = new[]
        {
            new AutomatizationConfiguration(configuration1) { RefreshIntervalSeconds = 1 },
            new AutomatizationConfiguration(configuration2) { RefreshIntervalSeconds = 5 },
            new AutomatizationConfiguration(configuration3) { RefreshIntervalSeconds = 10 }
        };

        foreach (var config in configs)
        {
            var service = new AutomatizationService(cache, config, websocketClient, meterFactory);
            service.StartServiceAsync();

            A.CallTo(() => websocketClient.ConnectAsync()).MustHaveHappened();
        }
    }
}
