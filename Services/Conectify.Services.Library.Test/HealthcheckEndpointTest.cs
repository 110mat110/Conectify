using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Conectify.Services.Library.Test;

/// <summary>
/// The endpoint is put into every service by the library, so it is worth checking that it really
/// answers - and that it answers with the state the watchdog put there.
/// </summary>
public class HealthcheckEndpointTest
{
    private const int NotHealthEndpoint = StatusCodes.Status418ImATeapot;

    private static (RequestDelegate Pipeline, IServiceProvider Services) BuildPipeline(IServicesWebsocketClient client, IConnectionState state)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(client);
        services.AddSingleton(state);
        services.AddHealthChecks().AddCheck<WebsocketHealthcheck>("websocket", HealthStatus.Unhealthy);
        var provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);
        // everything behind the healthcheck middleware, standing in for the service's own pipeline
        new HealthcheckStartupFilter().Configure(rest => rest.Run(context =>
        {
            context.Response.StatusCode = NotHealthEndpoint;
            return Task.CompletedTask;
        }))(app);

        return (app.Build(), provider);
    }

    private static async Task<int> CallAsync(RequestDelegate pipeline, IServiceProvider services, string path)
    {
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        await pipeline(context);

        return context.Response.StatusCode;
    }

    private static ConnectionState CreateState(ConfigurationBase configuration) => new(configuration, TimeProvider.System);

    [Fact]
    public async Task ItShallAnswerOnHealthWithoutTheServiceMappingIt()
    {
        var client = A.Fake<IServicesWebsocketClient>();
        A.CallTo(() => client.WebSocketState).Returns(WebSocketState.Open);
        var (pipeline, services) = BuildPipeline(client, CreateState(new ConfigurationBase(new ConfigurationBuilder().Build())));

        Assert.Equal(StatusCodes.Status200OK, await CallAsync(pipeline, services, HealthcheckStartupFilter.Path));
    }

    [Fact]
    public async Task ItShallLeaveTheRestOfThePipelineAlone()
    {
        var client = A.Fake<IServicesWebsocketClient>();
        A.CallTo(() => client.WebSocketState).Returns(WebSocketState.Open);
        var (pipeline, services) = BuildPipeline(client, CreateState(new ConfigurationBase(new ConfigurationBuilder().Build())));

        Assert.Equal(NotHealthEndpoint, await CallAsync(pipeline, services, "/api/something"));
    }

    [Fact]
    public async Task ItShallAnswerIllWhenTheSocketIsOpenButTheServerIsNotAnswering()
    {
        var client = A.Fake<IServicesWebsocketClient>();
        A.CallTo(() => client.WebSocketState).Returns(WebSocketState.Open);
        var configuration = new ConfigurationBase(new ConfigurationBuilder().Build());
        configuration.Watchdog.FailuresBeforeIll = 2;
        var state = CreateState(configuration);
        var (pipeline, services) = BuildPipeline(client, state);

        state.ReportFailedCheck("no answer");
        // one failure is not enough yet - degraded still answers 200 so nothing is restarted too early
        Assert.Equal(StatusCodes.Status200OK, await CallAsync(pipeline, services, HealthcheckStartupFilter.Path));

        state.ReportFailedCheck("no answer");
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, await CallAsync(pipeline, services, HealthcheckStartupFilter.Path));
    }

    [Fact]
    public async Task ItShallAnswerIllWhenTheSocketIsNotOpen()
    {
        var client = A.Fake<IServicesWebsocketClient>();
        A.CallTo(() => client.WebSocketState).Returns(WebSocketState.Closed);
        var (pipeline, services) = BuildPipeline(client, CreateState(new ConfigurationBase(new ConfigurationBuilder().Build())));

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, await CallAsync(pipeline, services, HealthcheckStartupFilter.Path));
    }
}
