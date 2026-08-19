using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class FunctionPortRuleBehaviourTests
{
    private readonly IAutomatizationCache cache = A.Fake<IAutomatizationCache>();
    private readonly IServiceProvider serviceProvider;

    public FunctionPortRuleBehaviourTests()
    {
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));
        A.CallTo(() => cache.GetPreviousOutputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(A.Fake<IMeterFactory>());
        serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Execute_ValuePort_CarriesTheEventToTheOutput()
    {
        var behaviour = new FunctionInputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value }],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent { NumericValue = 21, StringValue = "warm" }, CancellationToken.None);

        Assert.NotNull(output.Event);
        Assert.Equal(21, output.Event!.NumericValue);
        Assert.Equal("warm", output.Event.StringValue);
    }

    [Fact]
    public async Task Execute_ValuePort_DoesNotRunTheRulesBehindIt()
    {
        var behaviour = new FunctionInputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value }],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent { NumericValue = 1 }, CancellationToken.None);

        // A value only refreshes what readers pull, so nothing downstream is looked up.
        A.CallTo(() => cache.GetNextInputs(output.Id)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Execute_TriggerPort_PassesThePulseOn()
    {
        var behaviour = new FunctionInputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Trigger }],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent { Type = "Trigger" }, CancellationToken.None);

        A.CallTo(() => cache.GetNextInputs(output.Id)).MustHaveHappened();
    }

    [Fact]
    public async Task InitializationValue_MakesThePortReactToValues()
    {
        var behaviour = new FunctionOutputRuleBehaviour(serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value }],
            Outputs = [new OutputPointDTO(Guid.NewGuid(), serviceProvider)]
        };

        await behaviour.InitializationValue(rule, null);

        // Without this a value reaching the port would be dropped instead of carried over.
        Assert.True(rule.TriggerOnValue);
    }

    [Fact]
    public async Task InitializationValue_TakesOverTheValueThatFeedsThePort()
    {
        var behaviour = new FunctionInputRuleBehaviour(serviceProvider);
        var input = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value };
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);

        A.CallTo(() => cache.GetPreviousOutputs(input.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([
            new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 5 } }
        ]));

        var rule = new RuleDTO { Id = Guid.NewGuid(), Inputs = [input], Outputs = [output] };

        await behaviour.InitializationValue(rule, null);

        Assert.NotNull(output.Event);
        Assert.Equal(5, output.Event!.NumericValue);
    }

    [Fact]
    public void Ports_AreNotOfferedAsPlainRules()
    {
        Assert.IsAssignableFrom<IInternalRuleBehaviour>(new FunctionInputRuleBehaviour(serviceProvider));
        Assert.IsAssignableFrom<IInternalRuleBehaviour>(new FunctionOutputRuleBehaviour(serviceProvider));
    }

    [Fact]
    public void SetParameters_NamesThePortAfterItsOptions()
    {
        var behaviour = new FunctionInputRuleBehaviour(serviceProvider);
        var rule = new Rule { Id = Guid.NewGuid(), ParametersJson = "{\"Name\":\"Temperature\"}" };

        behaviour.SetParameters(rule, CancellationToken.None).Wait();

        Assert.Equal("Temperature", rule.Name);
        Assert.Equal("Function input: Temperature", rule.Description);
    }
}
