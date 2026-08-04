using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Services;

public class AutomatizationServiceTests
{
    private static AutomatizationService CreateService(IServicesWebsocketClient websocketClient, IRuleScheduler scheduler)
    {
        return new AutomatizationService(
            A.Fake<IAutomatizationCache>(),
            scheduler,
            websocketClient,
            A.Fake<IMeterFactory>(),
            A.Fake<ILogger<AutomatizationService>>());
    }

    [Fact]
    public void StartServiceAsync_ShouldConnectWebsocket()
    {
        var websocketClient = A.Fake<IServicesWebsocketClient>();

        CreateService(websocketClient, A.Fake<IRuleScheduler>()).StartServiceAsync();

        A.CallTo(() => websocketClient.ConnectAsync()).MustHaveHappened();
    }

    [Fact]
    public void StartServiceAsync_ShouldStartTheRuleScheduler()
    {
        var scheduler = A.Fake<IRuleScheduler>();

        CreateService(A.Fake<IServicesWebsocketClient>(), scheduler).StartServiceAsync();

        A.CallTo(() => scheduler.Start()).MustHaveHappened();
    }

    [Fact]
    public void Constructor_ShouldAcceptAllParameters()
    {
        var service = CreateService(A.Fake<IServicesWebsocketClient>(), A.Fake<IRuleScheduler>());

        Assert.NotNull(service);
    }
}
