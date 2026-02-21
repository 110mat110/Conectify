using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class UserInputRuleBehaviourTests
{
    [Fact]
    public void GetId_ShouldReturnCorrectGuid()
    {
        var behaviour = new UserInputRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        Assert.Equal(Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925798"), behaviour.GetId());
    }

    [Fact]
    public void DisplayName_ShouldReturnUserInput()
    {
        var behaviour = new UserInputRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        Assert.Equal("USER INPUT", behaviour.DisplayName());
    }

    [Fact]
    public void Outputs_ShouldHaveCorrectConfiguration()
    {
        var behaviour = new UserInputRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        var outputs = behaviour.Outputs;

        Assert.Equal(1, outputs.Min);
        Assert.Equal(1, outputs.Def);
        Assert.Equal(1, outputs.Max);
    }

    [Fact]
    public async Task Execute_ShouldSetOutputs()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new UserInputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO { Outputs = [output] };
        var trigger = new AutomatisationEvent { Name = "test", NumericValue = 1 };

        await behaviour.Execute(rule, trigger);

        Assert.Same(trigger, output.Event);
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

        var behaviour = new UserInputRuleBehaviour(serviceProvider);
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { SourceActuatorId = actuatorId })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Equal(actuatorName, rule.Name);
        Assert.Equal($"Source: {actuatorName}", rule.Description);
        Assert.Contains(actuatorId.ToString(), rule.ParametersJson);
        Assert.Contains(actuatorName, rule.ParametersJson);
    }
}
