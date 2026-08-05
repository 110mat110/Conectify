using System.Net.WebSockets;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Conectify.Services.Library.Test;

public class ConnectionWatchdogTest
{
    private static readonly DateTimeOffset Now = new(2026, 8, 5, 12, 0, 0, TimeSpan.Zero);

    /// <summary>A clock the test moves by hand; delays stay real (and the tests keep them at zero).</summary>
    private sealed class FrozenTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static ConfigurationBase CreateConfiguration(Action<WatchdogConfiguration>? setup = null)
    {
        var configuration = new ConfigurationBase(new ConfigurationBuilder().Build());
        // Probe and answer within the same test tick unless a test says otherwise.
        configuration.Watchdog.SilenceThresholdSeconds = 0;
        configuration.Watchdog.ProbeTimeoutSeconds = 0;
        configuration.Watchdog.RestartAfterSeconds = 0;
        setup?.Invoke(configuration.Watchdog);
        return configuration;
    }

    private static ConnectionWatchdog CreateWatchdog(
        IServicesWebsocketClient client,
        IConnectionState state,
        ConfigurationBase configuration,
        TimeProvider timeProvider,
        IServiceRestarter? restarter = null)
        => new(client, state, restarter ?? A.Fake<IServiceRestarter>(), configuration, timeProvider, A.Fake<ILogger<ConnectionWatchdog>>());

    private static IServicesWebsocketClient CreateOpenClient()
    {
        var client = A.Fake<IServicesWebsocketClient>();
        A.CallTo(() => client.WebSocketState).Returns(WebSocketState.Open);
        A.CallTo(() => client.SendMessageAsync(A<WebsocketEvent>._, A<CancellationToken>._)).Returns(true);
        return client;
    }

    [Fact]
    public async Task ItShallNotProbeWhileEventsAreFlowing()
    {
        var client = CreateOpenClient();
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration(w => w.SilenceThresholdSeconds = 120);
        var state = new ConnectionState(configuration, time);
        state.ReportContact();

        await CreateWatchdog(client, state, configuration, time).RunCheckAsync(default);

        A.CallTo(() => client.SendMessageAsync(A<WebsocketEvent>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => client.ForceReconnectAsync(A<CancellationToken>._)).MustNotHaveHappened();
        Assert.Equal(ConnectionQuality.Healthy, state.Quality);
    }

    [Fact]
    public async Task ItShallProbeItselfThroughTheServerWhenTheServerGoesQuiet()
    {
        var client = CreateOpenClient();
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration();
        var state = new ConnectionState(configuration, time);
        // the server answers - anything coming back counts
        A.CallTo(() => client.SendMessageAsync(A<WebsocketEvent>._, A<CancellationToken>._))
            .Invokes(() => state.ReportContact())
            .Returns(true);

        await CreateWatchdog(client, state, configuration, time).RunCheckAsync(default);

        A.CallTo(() => client.SendMessageAsync(
            A<WebsocketEvent>.That.Matches(x => x.Name == Constants.Commands.ConnectionCheck
                && x.Type == Constants.Events.Command
                && x.DestinationId == configuration.DeviceId),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => client.ForceReconnectAsync(A<CancellationToken>._)).MustNotHaveHappened();
        Assert.Equal(ConnectionQuality.Healthy, state.Quality);
    }

    [Fact]
    public async Task ItShallReconnectWhenTheOpenSocketIsNotAnswering()
    {
        var client = CreateOpenClient();
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration();
        var state = new ConnectionState(configuration, time);

        await CreateWatchdog(client, state, configuration, time).RunCheckAsync(default);

        A.CallTo(() => client.ForceReconnectAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        Assert.Equal(1, state.ConsecutiveFailures);
        Assert.Equal(Now, state.FailingSince);
    }

    [Fact]
    public async Task ItShallReconnectWhenTheSocketIsNotOpenWithoutProbing()
    {
        var client = A.Fake<IServicesWebsocketClient>();
        A.CallTo(() => client.WebSocketState).Returns(WebSocketState.Aborted);
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration();
        var state = new ConnectionState(configuration, time);

        await CreateWatchdog(client, state, configuration, time).RunCheckAsync(default);

        A.CallTo(() => client.SendMessageAsync(A<WebsocketEvent>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => client.ForceReconnectAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        Assert.Contains("Aborted", state.LastFailure);
    }

    [Fact]
    public async Task ItShallTurnTheHealthcheckIllWhenTheServerKeepsSilent()
    {
        var client = CreateOpenClient();
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration(w => w.FailuresBeforeIll = 2);
        var state = new ConnectionState(configuration, time);
        var watchdog = CreateWatchdog(client, state, configuration, time);
        var healthcheck = new WebsocketHealthcheck(client, state);

        Assert.Equal(HealthStatus.Healthy, (await healthcheck.CheckHealthAsync(new HealthCheckContext())).Status);

        await watchdog.RunCheckAsync(default);
        Assert.Equal(HealthStatus.Degraded, (await healthcheck.CheckHealthAsync(new HealthCheckContext())).Status);

        await watchdog.RunCheckAsync(default);
        Assert.Equal(HealthStatus.Unhealthy, (await healthcheck.CheckHealthAsync(new HealthCheckContext())).Status);
    }

    [Fact]
    public async Task ItShallGoBackToHealthyWhenTheServerAnswersAgain()
    {
        var client = CreateOpenClient();
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration(w => w.FailuresBeforeIll = 1);
        var state = new ConnectionState(configuration, time);
        var watchdog = CreateWatchdog(client, state, configuration, time);
        var healthcheck = new WebsocketHealthcheck(client, state);

        await watchdog.RunCheckAsync(default);
        Assert.Equal(HealthStatus.Unhealthy, (await healthcheck.CheckHealthAsync(new HealthCheckContext())).Status);

        A.CallTo(() => client.SendMessageAsync(A<WebsocketEvent>._, A<CancellationToken>._))
            .Invokes(() => state.ReportContact())
            .Returns(true);
        await watchdog.RunCheckAsync(default);

        Assert.Equal(HealthStatus.Healthy, (await healthcheck.CheckHealthAsync(new HealthCheckContext())).Status);
        Assert.Null(state.FailingSince);
    }

    [Fact]
    public async Task ItShallRestartTheServiceWhenReconnectingKeepsNotHelping()
    {
        var client = CreateOpenClient();
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration(w => w.RestartAfterSeconds = 300);
        var state = new ConnectionState(configuration, time);
        var restarter = A.Fake<IServiceRestarter>();
        var watchdog = CreateWatchdog(client, state, configuration, time, restarter);

        await watchdog.RunCheckAsync(default);
        A.CallTo(() => restarter.RestartAsync(A<string>._)).MustNotHaveHappened();

        time.Now = Now.AddMinutes(6);
        await watchdog.RunCheckAsync(default);

        A.CallTo(() => restarter.RestartAsync(A<string>._)).MustHaveHappenedOnceExactly();
        // reconnecting was tried first and is not tried again once we give up on this process
        A.CallTo(() => client.ForceReconnectAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ItShallOnlyReconnectWhenRestartingIsTurnedOff()
    {
        var client = CreateOpenClient();
        var time = new FrozenTimeProvider(Now);
        var configuration = CreateConfiguration(w => w.RestartAfterSeconds = 0);
        var state = new ConnectionState(configuration, time);
        var restarter = A.Fake<IServiceRestarter>();
        var watchdog = CreateWatchdog(client, state, configuration, time, restarter);

        await watchdog.RunCheckAsync(default);
        time.Now = Now.AddHours(5);
        await watchdog.RunCheckAsync(default);

        A.CallTo(() => restarter.RestartAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => client.ForceReconnectAsync(A<CancellationToken>._)).MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public void ItShallReadWatchdogSettingsFromConfiguration()
    {
        var configuration = new ConfigurationBase(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Watchdog:Enabled", "false" },
            { "Watchdog:SilenceThresholdSeconds", "42" },
            { "Watchdog:RestartAfterSeconds", "0" },
        }).Build());

        Assert.False(configuration.Watchdog.Enabled);
        Assert.Equal(42, configuration.Watchdog.SilenceThresholdSeconds);
        Assert.Equal(0, configuration.Watchdog.RestartAfterSeconds);
        Assert.Equal(30, configuration.Watchdog.CheckIntervalSeconds);
    }
}
