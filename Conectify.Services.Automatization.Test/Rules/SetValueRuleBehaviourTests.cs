using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class SetValueRuleBehaviourTests
{
    [Fact]
    public async Task Execute_WithParameters_ShouldSetOutputs()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new SetValueRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Outputs = [output],
            ParametersJson = JsonConvert.SerializeObject(new { NumericValue = 42f, StringValue = "test", Unit = "u" })
        };

        await behaviour.Execute(rule, new AutomatisationEvent());

        Assert.NotNull(output.Event);
        Assert.Equal(42f, output.Event!.NumericValue);
        Assert.Equal("test", output.Event.StringValue);
        Assert.Equal("u", output.Event.Unit);
    }

    [Fact]
    public async Task InitializationValue_WithParameters_ShouldSetOutputs()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new SetValueRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            Outputs = [output],
            ParametersJson = JsonConvert.SerializeObject(new { NumericValue = 5f, StringValue = "init", Unit = "m" })
        };

        await behaviour.InitializationValue(rule, null);

        Assert.NotNull(output.Event);
        Assert.Equal(5f, output.Event!.NumericValue);
        Assert.Equal("init", output.Event.StringValue);
        Assert.Equal("m", output.Event.Unit);
    }
}
