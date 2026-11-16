using System.Net.WebSockets;
using System.Text;
using Conectify.Server.Caches;
using Conectify.Server.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Server.Test.Services;

public class WebSocketServiceTest
{
    private readonly ISubscribersCache subscribersCache = A.Fake<ISubscribersCache>();
    private readonly IDataService dataService = A.Fake<IDataService>();
    private readonly IWebsocketCache websocketCache;
    private readonly IServiceProvider serviceProvider;

    public WebSocketServiceTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(dataService);
        services.AddScoped<System.Diagnostics.Metrics.IMeterFactory, Cahces.WebsocketCacheTest.FakeMeterFactory>();
        serviceProvider = services.BuildServiceProvider();
        websocketCache = new WebsocketCache(serviceProvider, A.Fake<ILogger<WebsocketCache>>());
    }

    private WebSocketService CreateService(IWebsocketCache? cache = null) =>
        new(A.Fake<ILogger<WebSocketService>>(), subscribersCache, serviceProvider, A.Fake<IDeviceService>(), cache ?? websocketCache)
        {
            SendTimeout = TimeSpan.FromMilliseconds(200),
        };

    [Fact]
    public async Task ItShallGiveUpOnDeviceThatStoppedReading()
    {
        var deviceId = Guid.NewGuid();
        var socket = WebSocket.CreateFromStream(new StalledStream(), isServer: true, subProtocol: null, keepAliveInterval: Timeout.InfiniteTimeSpan);
        var cache = A.Fake<IWebsocketCache>();
        A.CallTo(() => cache.GetActiveSocket(deviceId)).Returns(socket);
        var service = CreateService(cache);

        var send = service.SendToDeviceAsync(deviceId, "{}");
        var finished = await Task.WhenAny(send, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.Same(send, finished);
        Assert.False(await send);
        Assert.Equal(WebSocketState.Aborted, socket.State);
    }

    [Fact]
    public async Task ItShallCleanUpConnectionThatBroke()
    {
        var deviceId = Guid.NewGuid();
        var socket = new ScriptedWebSocket().Breaks();

        await CreateService().ConnectAsync(deviceId, socket);

        Assert.Equal(0, websocketCache.GetNoOfActiveSockets(deviceId));
        Assert.False(websocketCache.IsActiveSocket(deviceId));
        A.CallTo(() => subscribersCache.RemoveSubscriber(deviceId)).MustHaveHappened();
    }

    [Fact]
    public async Task ItShallNotCloseNewerConnectionWhenOlderOneEnds()
    {
        var deviceId = Guid.NewGuid();
        var oldConnectionEnds = new TaskCompletionSource();
        var oldSocket = new ScriptedWebSocket().WaitsFor(oldConnectionEnds.Task).ReceivesClose();
        var newSocket = new ScriptedWebSocket();

        var oldConnection = CreateService().ConnectAsync(deviceId, oldSocket);
        // the device reconnects before the server notices that its old connection is gone
        websocketCache.AddNewWebsocket(deviceId, newSocket);
        oldConnectionEnds.SetResult();
        await oldConnection;

        Assert.False(newSocket.WasClosed);
        Assert.Same(newSocket, websocketCache.GetActiveSocket(deviceId));
        A.CallTo(() => subscribersCache.RemoveSubscriber(deviceId)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ItShallProcessMessageThatCameInPieces()
    {
        var deviceId = Guid.NewGuid();
        var json = "{\"Type\":\"Value\",\"Name\":\"Temperature\",\"NumericValue\":21.5}";
        var socket = new ScriptedWebSocket()
            .Receives(json[..20], endOfMessage: false)
            .Receives(json[20..])
            .ReceivesClose();

        await CreateService().ConnectAsync(deviceId, socket);

        A.CallTo(() => dataService.InsertJsonModel(json, deviceId, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => dataService.InsertJsonModel(A<string>.Ignored, A<Guid>.Ignored, A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
    }

    /// <summary>
    /// The connection of a device that does not read anymore: nothing written to it ever gets through, until the
    /// connection is torn down.
    /// </summary>
    private sealed class StalledStream : Stream
    {
        private readonly TaskCompletionSource closed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await closed.Task;
            throw new IOException("Connection was torn down");
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await closed.Task;
            return 0;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override void Flush() { }
        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            closed.TrySetResult();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Plays back what a device sends, in order, and remembers whether the server closed it. Once the script runs
    /// out, it waits like a connected device that has nothing to say.
    /// </summary>
    private sealed class ScriptedWebSocket : WebSocket
    {
        private readonly Queue<(Task Before, Func<ArraySegment<byte>, WebSocketReceiveResult> Step)> script = new();
        private Task nextWaitsFor = Task.CompletedTask;
        private WebSocketState state = WebSocketState.Open;

        public bool WasClosed { get; private set; }

        public ScriptedWebSocket Receives(string text, bool endOfMessage = true) => Then(buffer =>
        {
            var count = Encoding.UTF8.GetBytes(text, buffer.AsSpan());
            return new WebSocketReceiveResult(count, WebSocketMessageType.Text, endOfMessage);
        });

        public ScriptedWebSocket ReceivesClose() => Then(_ =>
        {
            state = WebSocketState.CloseReceived;
            return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, "");
        });

        public ScriptedWebSocket Breaks() => Then(_ =>
        {
            state = WebSocketState.Aborted;
            throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely);
        });

        /// <summary>The next step of the script happens only once the task completes.</summary>
        public ScriptedWebSocket WaitsFor(Task task)
        {
            nextWaitsFor = task;
            return this;
        }

        private ScriptedWebSocket Then(Func<ArraySegment<byte>, WebSocketReceiveResult> step)
        {
            script.Enqueue((nextWaitsFor, step));
            nextWaitsFor = Task.CompletedTask;
            return this;
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            if (script.TryDequeue(out var next))
            {
                await next.Before;
                return next.Step(buffer);
            }

            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new OperationCanceledException();
        }

        public override WebSocketCloseStatus? CloseStatus => null;
        public override string? CloseStatusDescription => null;
        public override WebSocketState State => state;
        public override string? SubProtocol => null;

        public override void Abort()
        {
            WasClosed = true;
            state = WebSocketState.Aborted;
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            WasClosed = true;
            state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            WasClosed = true;
            state = state == WebSocketState.CloseReceived ? WebSocketState.Closed : WebSocketState.CloseSent;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            WasClosed = true;
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
