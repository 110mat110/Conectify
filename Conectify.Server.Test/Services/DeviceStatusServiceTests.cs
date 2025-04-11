using Conectify.Server;
using Conectify.Server.Caches;
using Conectify.Server.Services;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Conectify.Server.Test.Services;

public class DeviceStatusServiceTests
{
    private readonly ISubscribersCache fakeSubscribersCache;
    private readonly Configuration fakeConfiguration;
    private readonly IServiceScopeFactory fakeDataService;

    public DeviceStatusServiceTests()
    {
        this.fakeDataService = A.Fake<IServiceScopeFactory>();
        this.fakeSubscribersCache = A.Fake<ISubscribersCache>();
        this.fakeConfiguration = A.Fake<Configuration>();
    }

    private DeviceStatusService CreateService()
    {
        return new DeviceStatusService(
            this.fakeDataService,
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
