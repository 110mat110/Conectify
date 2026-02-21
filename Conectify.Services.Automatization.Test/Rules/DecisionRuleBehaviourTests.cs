using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class DecisionRuleBehaviourTests
{
    [Fact]
    public async Task Execute_WithMatchingCondition_ShouldSetOutput()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var logger = A.Fake<ILogger<DecisionRuleBehaviour>>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(logger);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new DecisionRuleBehaviour(serviceProvider);
        var parametersJson = JsonConvert.SerializeObject(new { Rule = ">" });

        var param1 = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Parameter };
        var param2 = new InputPointDTO { Id = Guid.NewGuid(), Index = 1, Type = InputTypeEnum.Parameter };
        var valueInput = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value };

        var param1Output = new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 10 } };
        var param2Output = new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 5 } };
        var valueOutput = new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 123, StringValue = "value" } };

        A.CallTo(() => cache.GetPreviousOutputs(param1.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([param1Output]));
        A.CallTo(() => cache.GetPreviousOutputs(param2.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([param2Output]));
        A.CallTo(() => cache.GetPreviousOutputs(valueInput.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([valueOutput]));

        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = parametersJson,
            Inputs = [param1, param2, valueInput],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent());

        Assert.NotNull(output.Event);
        Assert.Equal(123, output.Event!.NumericValue);
        Assert.Equal("Comparsion output", output.Event.Name);
    }

    [Fact]
    public async Task Execute_WithNonMatchingCondition_ShouldNotSetOutput()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var logger = A.Fake<ILogger<DecisionRuleBehaviour>>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(logger);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new DecisionRuleBehaviour(serviceProvider);
        var parametersJson = JsonConvert.SerializeObject(new { Rule = "<" });

        var param1 = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Parameter };
        var param2 = new InputPointDTO { Id = Guid.NewGuid(), Index = 1, Type = InputTypeEnum.Parameter };
        var valueInput = new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Value };

        var param1Output = new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 10 } };
        var param2Output = new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 5 } };
        var valueOutput = new OutputPointDTO(Guid.NewGuid(), serviceProvider) { Event = new AutomatisationEvent { NumericValue = 123 } };

        A.CallTo(() => cache.GetPreviousOutputs(param1.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([param1Output]));
        A.CallTo(() => cache.GetPreviousOutputs(param2.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([param2Output]));
        A.CallTo(() => cache.GetPreviousOutputs(valueInput.Id)).Returns(Task.FromResult<IEnumerable<OutputPointDTO>>([valueOutput]));

        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = parametersJson,
            Inputs = [param1, param2, valueInput],
            Outputs = [output]
        };

        await behaviour.Execute(rule, new AutomatisationEvent());

        Assert.Null(output.Event);
    }

    [Fact]
    public async Task SetParameters_WithIncorrectParameterCount_ShouldSetErrorDescription()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var logger = A.Fake<ILogger<DecisionRuleBehaviour>>();

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(logger);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new DecisionRuleBehaviour(serviceProvider);
        var ruleId = Guid.NewGuid();
        var rule = new Rule { Id = ruleId, ParametersJson = JsonConvert.SerializeObject(new { Rule = ">" }) };

        var ruleDto = new RuleDTO
        {
            Inputs = [new InputPointDTO { Id = Guid.NewGuid(), Index = 0, Type = InputTypeEnum.Parameter }]
        };

        A.CallTo(() => cache.GetRuleByIdAsync(ruleId, A<CancellationToken>._))
            .Returns(Task.FromResult<RuleDTO?>(ruleDto));

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Equal("You need EXACTLY 2 Parameters", rule.Description);
    }
}
