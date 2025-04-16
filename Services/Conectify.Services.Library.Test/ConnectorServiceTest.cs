using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Services;


namespace Conectify.Services.Library.Test;

public class ConnectorServiceTest
{
    readonly IConfiguration Configuration;
    const string serverUrl = "test.test";
    public ConnectorServiceTest()
    {
        var myConfiguration = new Dictionary<string, string?>
    {
        {"ServerUrl", serverUrl}
    };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }


    [Fact]
    public async Task ItShallRegisterOnlyDevice()
    {
        var client = A.Fake<HttpClient>();
        var provider = A.Fake<IHttpFactory>();
        A.CallTo(() => provider.HttpClient).Returns(client);
        var service = new ConnectorService(A.Fake<ILogger<ConnectorService>>(), new ConfigurationBase(Configuration), A.Fake<IMapper>(), provider);

        await service.RegisterDevice(new ApiDevice(), [], []);

        var resultUri = serverUrl + "/api/device";
        A.CallTo(() => client.SendAsync(A<HttpRequestMessage>.That.Matches(x => x.Method.Method == HttpMethod.Post.Method && x.RequestUri!.OriginalString == resultUri), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ItShallRegisterAllDevices()
    {
        var client = A.Fake<HttpClient>();
        var provider = A.Fake<IHttpFactory>();
        A.CallTo(() => provider.HttpClient).Returns(client);
        var service = new ConnectorService(A.Fake<ILogger<ConnectorService>>(), new ConfigurationBase(Configuration), A.Fake<IMapper>(), provider);

        await service.RegisterDevice(new ApiDevice(), [new(), new(), new()], [new(), new()]);

        var deviceResultUrl = serverUrl + "/api/device";
        var sensorResultUrl = serverUrl + "/api/sensors";
        var actuatorsResultUrl = serverUrl + "/api/actuators";
        A.CallTo(() => client.SendAsync(A<HttpRequestMessage>.That.Matches(x => x.Method.Method == HttpMethod.Post.Method && x.RequestUri!.OriginalString == deviceResultUrl), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => client.SendAsync(A<HttpRequestMessage>.That.Matches(x => x.Method.Method == HttpMethod.Post.Method && x.RequestUri!.OriginalString == sensorResultUrl), A<CancellationToken>.Ignored)).MustHaveHappened(3, Times.Exactly);
        A.CallTo(() => client.SendAsync(A<HttpRequestMessage>.That.Matches(x => x.Method.Method == HttpMethod.Post.Method && x.RequestUri!.OriginalString == actuatorsResultUrl), A<CancellationToken>.Ignored)).MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public async Task ItShallSetNonePreferences()
    {
        var provider = A.Fake<IHttpFactory>();
        var service = new ConnectorService(A.Fake<ILogger<ConnectorService>>(), new ConfigurationBase(Configuration), A.Fake<IMapper>(), provider);

        var result = await service.SetPreferences(Guid.NewGuid(), []);

        Assert.False(result);
    }

    [Fact]
    public async Task ItShallSetPreferences()
    {
        var client = A.Fake<HttpClient>();
        var provider = A.Fake<IHttpFactory>();
        A.CallTo(() => provider.HttpClient).Returns(client);
        var service = new ConnectorService(A.Fake<ILogger<ConnectorService>>(), new ConfigurationBase(Configuration), A.Fake<IMapper>(), provider);
        var deviceId = Guid.NewGuid();

        var result = await service.SetPreferences(deviceId, [new(), new()]);

        var subsribeUrl = serverUrl + "/api/subscribe/" + deviceId.ToString();
        A.CallTo(() => client.SendAsync(A<HttpRequestMessage>.That.Matches(x => x.Method.Method == HttpMethod.Post.Method && x.RequestUri!.OriginalString == subsribeUrl), A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();

        Assert.True(result);
    }

    [Fact]
    public async Task ItShallNotFailWhenNoMetadatas()
    {
        var provider = A.Fake<IHttpFactory>();
        var service = new ConnectorService(A.Fake<ILogger<ConnectorService>>(), new ConfigurationBase(Configuration), A.Fake<IMapper>(), provider);

        var result = await service.SendMetadataForDevice(Guid.NewGuid(), []);

        Assert.False(result);
    }
}