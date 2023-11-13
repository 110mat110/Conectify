using Conectify.Server;
using Conectify.Server.Caches;
using Conectify.Server.Services;
using FakeItEasy;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Conectify.Server.Test.Services;

public class DeviceStatusServiceTests
{
    private IPipelineService fakePipelineService;
    private ISubscribersCache fakeSubscribersCache;
    private Configuration fakeConfiguration;

    public DeviceStatusServiceTests()
    {
        this.fakePipelineService = A.Fake<IPipelineService>();
        this.fakeSubscribersCache = A.Fake<ISubscribersCache>();
        this.fakeConfiguration = A.Fake<Configuration>();
    }

    private DeviceStatusService CreateService()
    {
        return new DeviceStatusService(
            this.fakePipelineService,
            this.fakeSubscribersCache,
            this.fakeConfiguration);
    }

    [Fact]
    public async Task CheckIfAlive_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        await service.CheckIfAlive();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void Dispose_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        service.Dispose();

        // Assert
        Assert.True(true);
    }
}
