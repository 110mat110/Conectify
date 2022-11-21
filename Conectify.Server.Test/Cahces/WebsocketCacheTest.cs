using Conectify.Server.Caches;
using System.Net.WebSockets;

namespace Conectify.Server.Test.Cahces;

public class WebsocketCacheTest
{
    [Fact]
    public void ItShallAddNewWebsocket()
    {
        var cache = new WebsocketCache();
        var deviceId = Guid.NewGuid();
        var result = cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());

        Assert.True(result);

        Assert.Equal(1, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallUpdateExistingWebsocket()
    {
        var cache = new WebsocketCache();
        var deviceId = Guid.NewGuid();
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        var result = cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());

        Assert.False(result);

        Assert.Equal(2, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallRemoveExistingWebsocket()
    {
        var cache = new WebsocketCache();
        var deviceId = Guid.NewGuid();
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        cache.Remove(deviceId);

        Assert.Equal(0, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallDecreaseCountOfWebsockets()
    {
        var cache = new WebsocketCache();
        var deviceId = Guid.NewGuid();
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        cache.AddNewWebsocket(deviceId, A.Fake<WebSocket>());
        cache.Remove(deviceId);

        Assert.Equal(1, cache.GetNoOfActiveSockets(deviceId));
    }

    [Fact]
    public void ItShallNotFailRemoveNonExistingWebsocket()
    {
        var cache = new WebsocketCache();

        cache.Remove(Guid.NewGuid());
    }

    [Fact]
    public void ItShallReturnWebsocket()
    {
        var cache = new WebsocketCache();
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
        var cache = new WebsocketCache();
        var deviceId = Guid.NewGuid();
        var ws = A.Fake<WebSocket>();
        A.CallTo(() => ws.State).Returns(state);
        cache.AddNewWebsocket(deviceId, ws);
        var result = cache.GetActiveSocket(deviceId);

        Assert.Null(result);
    }
}
