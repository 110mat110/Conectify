using System.Diagnostics.Metrics;
using System.Net.WebSockets;
using Conectify.Server.Caches;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Server.Test.Cahces;

public class WebsocketCacheTest
{
    readonly IServiceProvider serviceProvider;
    readonly ILogger<WebsocketCache> loggerMock;
    public WebsocketCacheTest()
    {
        var serviceCollection = new ServiceCollection();

        // Add DI configurations if necessary 
        serviceCollection.AddScoped<System.Diagnostics.Metrics.IMeterFactory, FakeMeterFactory>();

        // Build ServiceProvider
        serviceProvider = serviceCollection.BuildServiceProvider();

        loggerMock = A.Fake<ILogger<WebsocketCache>>();

    }

    [Fact]
    public void ItShallAddNewWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        var result = cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());

        Assert.True(result);

        Assert.Equal(1, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallUpdateExistingWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        var result = cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());

        Assert.False(result);

        Assert.Equal(2, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public async Task ItShallRemoveExistingWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        var ws = A.Fake<WebSocket>();
        cache.AddNewWebsocket(deviceId, ws);
        await cache.Remove(deviceId, ws, default);

        Assert.Equal(0, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public async Task ItShallDecreaseCountOfWebsockets()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        var ws = A.Fake<WebSocket>();
        cache.AddNewWebsocket(deviceId, ws);
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        await cache.Remove(deviceId, ws, default);

        Assert.Equal(1, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public async Task ItShallNotFailRemoveNonExistingWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);

        await cache.Remove(Guid.NewGuid(), A.Fake<WebSocket>(), default);
    }

    [Fact]
    public async Task ItShallKeepNewerWebsocketWhenOlderIsRemoved()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        var older = A.Fake<WebSocket>();
        var newer = A.Fake<WebSocket>();
        A.CallTo(() => older.State).Returns(WebSocketState.Open);
        A.CallTo(() => newer.State).Returns(WebSocketState.Open);
        cache.AddNewWebsocket(deviceId, older);
        cache.AddNewWebsocket(deviceId, newer);

        await cache.Remove(deviceId, older, default);

        Assert.Same(newer, cache.GetActiveSocket(deviceId));
        A.CallTo(() => older.CloseOutputAsync(A<WebSocketCloseStatus>._, A<string?>._, A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(newer).Where(call => call.Method.Name != "get_State").MustNotHaveHappened();
    }

    [Fact]
    public void ItShallFallBackToOlderWebsocketWhenNewerIsNotOpen()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        var older = A.Fake<WebSocket>();
        var newer = A.Fake<WebSocket>();
        A.CallTo(() => older.State).Returns(WebSocketState.Open);
        A.CallTo(() => newer.State).Returns(WebSocketState.Aborted);
        cache.AddNewWebsocket(deviceId, older);
        cache.AddNewWebsocket(deviceId, newer);

        Assert.Same(older, cache.GetActiveSocket(deviceId));
        Assert.True(cache.IsActiveSocket(deviceId));
    }

    [Fact]
    public void ItShallReturnWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        var ws = A.Fake<WebSocket>();
        A.CallTo(() => ws.State).Returns(WebSocketState.Open);
        cache.AddNewWebsocket(deviceId, ws);
        var result = cache.GetActiveSocket(deviceId);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(WebSocketState.Closed)]
    [InlineData(WebSocketState.Aborted)]
    [InlineData(WebSocketState.CloseReceived)]
    [InlineData(WebSocketState.CloseSent)]
    [InlineData(WebSocketState.Connecting)]
    [InlineData(WebSocketState.None)]
    public void ItShallNotReturnWebsocket(WebSocketState state)
    {
        var cache = new WebsocketCache(serviceProvider, loggerMock);
        var deviceId = Guid.NewGuid();
        var ws = A.Fake<WebSocket>();
        A.CallTo(() => ws.State).Returns(state);
        cache.AddNewWebsocket(deviceId, ws);
        var result = cache.GetActiveSocket(deviceId);

        Assert.Null(result);
    }

    public class FakeMeterFactory : System.Diagnostics.Metrics.IMeterFactory
    {
        public Meter Create(MeterOptions options)
        {
            return new Meter(options.Name ?? "TestMeter");
        }

        public void Dispose()
        {
            // no-op
        }
    }
}
