using Conectify.Services.Automatization;
using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

public class OutputRuleBehaviourTests
{
    [Fact]
    public async Task Execute_WithTriggerValue_ShouldSendWebsocketMessage()
    {
        var destinationId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var wsClient = A.Fake<IServicesWebsocketClient>();

        var configuration = new AutomatizationConfiguration(A.Fake<IConfiguration>()) { SensorId = sensorId };

        var services = new ServiceCollection();
        services.AddScoped(_ => wsClient);
        services.AddScoped(_ => configuration);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new OutputRuleBehaviour(serviceProvider);
        var rule = new RuleDTO
        {
            ParametersJson = JsonConvert.SerializeObject(new { DestinationId = destinationId })
        };

        var triggerValue = new AutomatisationEvent
        {
            Name = "Test",
            NumericValue = 5,
            StringValue = "value",
            Unit = "u",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        await behaviour.Execute(rule, triggerValue, CancellationToken.None);

        A.CallTo(() => wsClient.SendMessageAsync(
                A<WebsocketEvent>.That.Matches(e =>
                    e.DestinationId == destinationId &&
                    e.SourceId == sensorId &&
                    e.Type == Constants.Events.Action),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetParameters_WithValidActuator_ShouldUpdateRuleProperties()
    {
        var actuatorId = Guid.NewGuid();
        var actuatorName = "TestActuator";
        var connectorService = A.Fake<IConnectorService>();
        A.CallTo(() => connectorService.LoadActuator(actuatorId, A<CancellationToken>._))
            .Returns(Task.FromResult<ApiActuator?>(new ApiActuator { Id = actuatorId, Name = actuatorName }));

        var services = new ServiceCollection();
        services.AddSingleton(connectorService);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new OutputRuleBehaviour(serviceProvider);
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { DestinationId = actuatorId })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Equal(actuatorName, rule.Name);
        Assert.Equal($"Destintaion: {actuatorName}", rule.Description);
    }
}
