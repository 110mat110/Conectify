using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class AndRuleBehaviourTests
{
    [Fact]
    public async Task Execute_WithAllTrueInputs_ShouldOutputTrue()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new AndRuleBehaviour(serviceProvider);
        var input1 = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value };
        var input2 = new InputPointDTO { Id = Guid.NewGuid(), Index = 1, Type = InputTypeEnum.Value };

        A.CallTo(() => cache.GetPreviousOutputs(input1.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([
            new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 1 } }
        ]));
        A.CallTo(() => cache.GetPreviousOutputs(input2.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([
            new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 2 } }
        ]));

        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [input1, input2],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        Assert.NotNull(output.Event);
        Assert.Equal(1, output.Event!.NumericValue);
        Assert.Equal("true", output.Event.StringValue);
    }

    [Fact]
    public async Task Execute_WithFalseInput_ShouldOutputFalse()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new AndRuleBehaviour(serviceProvider);
        var input1 = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value };
        var input2 = new InputPointDTO { Id = Guid.NewGuid(), Index = 1, Type = InputTypeEnum.Value };

        A.CallTo(() => cache.GetPreviousOutputs(input1.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([
            new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 0 } }
        ]));
        A.CallTo(() => cache.GetPreviousOutputs(input2.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([
            new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 2 } }
        ]));

        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [input1, input2],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        Assert.NotNull(output.Event);
        Assert.Equal(0, output.Event!.NumericValue);
        Assert.Equal("false", output.Event.StringValue);
    }

    [Fact]
    public async Task InitializationValue_ShouldSetFalseOutput()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new AndRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO { Id = Guid.NewGuid(), Outputs = [output] };

        await behaviour.InitializationValue(rule, null);

        Assert.NotNull(output.Event);
        Assert.Equal(0, output.Event!.NumericValue);
        Assert.Equal("false", output.Event.StringValue);
    }

    [Fact]
    public void GetId_ShouldReturnConsistentGuid()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour1 = new AndRuleBehaviour(serviceProvider);
        var behaviour2 = new AndRuleBehaviour(serviceProvider);

        Assert.Equal(behaviour1.GetId(), behaviour2.GetId());
        Assert.NotEqual(Guid.Empty, behaviour1.GetId());
    }

    [Fact]
    public void DisplayName_ShouldReturnAND()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour = new AndRuleBehaviour(serviceProvider);

        Assert.Equal("AND", behaviour.DisplayName());
    }

    [Fact]
    public void Inputs_ShouldHaveCorrectConfiguration()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour = new AndRuleBehaviour(serviceProvider);

        Assert.NotEmpty(behaviour.Inputs);
        Assert.All(behaviour.Inputs, input =>
        {
            Assert.True(input.Item2.Min >= 0);
            Assert.True(input.Item2.Max >= input.Item2.Min);
            Assert.True(input.Item2.Def >= input.Item2.Min && input.Item2.Def <= input.Item2.Max);
        });
    }

    [Fact]
    public void Outputs_ShouldHaveCorrectConfiguration()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour = new AndRuleBehaviour(serviceProvider);

        Assert.Equal(1, behaviour.Outputs.Min);
        Assert.Equal(1, behaviour.Outputs.Max);
        Assert.Equal(1, behaviour.Outputs.Def);
    }

    [Fact]
    public async Task Execute_WithNoInputs_ShouldOutputFalse()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new AndRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        Assert.NotNull(output.Event);
        Assert.Equal(0, output.Event!.NumericValue);
    }

    [Fact]
    public async Task Execute_WithNullInputEvent_ShouldHandleGracefully()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new AndRuleBehaviour(serviceProvider);
        var input1 = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value };

        A.CallTo(() => cache.GetPreviousOutputs(input1.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([]));

        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Inputs = [input1],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent(), CancellationToken.None);

        Assert.NotNull(output.Event);
        Assert.Equal(0, output.Event!.NumericValue);
    }

    [Fact]
    public async Task SetParameters_ShouldSetNameAndDescription()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour = new AndRuleBehaviour(serviceProvider);
        var rule = new Rule { Id = Guid.NewGuid(), ParametersJson = "{}" };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("AND", rule.Name);
        Assert.Contains("AND", rule.Description);
    }
}
