using Conectify.Server.Caches;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;

namespace Conectify.Server.Test.Cahces;

public class WebsocketCacheTest
{
    IServiceProvider serviceProvider;

    public WebsocketCacheTest()
    {
        var serviceCollection = new ServiceCollection();

        // Add DI configurations if necessary 
        serviceCollection.AddScoped<IMeterFactory, FakeMeterFactory> ();

        // Build ServiceProvider
        serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void ItShallAddNewWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider);
        var deviceId = Guid.NewGuid();
        var result = cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());

        Assert.True(result);

        Assert.Equal(1, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallUpdateExistingWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider);
        var deviceId = Guid.NewGuid();
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        var result = cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());

        Assert.False(result);

        Assert.Equal(2, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallRemoveExistingWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider);
        var deviceId = Guid.NewGuid();
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        cache.Remove(deviceId);

        Assert.Equal(0, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallDecreaseCountOfWebsockets()
    {
        var cache = new WebsocketCache(serviceProvider);
        var deviceId = Guid.NewGuid();
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        cache.Remove(deviceId);

        Assert.Equal(1, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallNotFailRemoveNonExistingWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider);

        cache.Remove(Guid.NewGuid());
    }

    [Fact]
    public void ItShallReturnWebsocket()
    {
        var cache = new WebsocketCache(serviceProvider);
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
        var cache = new WebsocketCache(serviceProvider);
        var deviceId = Guid.NewGuid();
        var ws = A.Fake<WebSocket>();
        A.CallTo(() => ws.State).Returns(state);
        cache.AddNewWebsocket(deviceId, ws);
        var result = cache.GetActiveSocket(deviceId);

        Assert.Null(result);
    }

    public class FakeMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options)
        {
            return new Meter(options);
        }

        public void Dispose()
        {
            return;
        }
    }
}
