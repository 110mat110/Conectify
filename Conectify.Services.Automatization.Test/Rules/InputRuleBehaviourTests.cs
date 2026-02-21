using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Test.Rules;

public class InputRuleBehaviourTests
{
    [Fact]
    public async Task Execute_WithMatchingEventType_ShouldSetOutputs()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Outputs = [output],
            ParametersJson = JsonConvert.SerializeObject(new { Event = "TestEvent", SourceSensorId = Guid.NewGuid() })
        };

        await behaviour.Execute(rule, new AutomatisationEvent { Type = "TestEvent" }, CancellationToken.None);

        Assert.NotNull(output.Event);
    }

    [Fact]
    public async Task Execute_WithAllEventsType_ShouldSetOutputs()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Outputs = [output],
            ParametersJson = JsonConvert.SerializeObject(new { Event = Constants.Events.All, SourceSensorId = Guid.NewGuid() })
        };

        await behaviour.Execute(rule, new AutomatisationEvent { Type = "AnyEvent" }, CancellationToken.None);

        Assert.NotNull(output.Event);
    }

    [Fact]
    public async Task SetParameters_WithValidSensor_ShouldUpdateRuleProperties()
    {
        var sensorId = Guid.NewGuid();
        var sensorName = "TestSensor";
        var connectorService = A.Fake<IConnectorService>();
        A.CallTo(() => connectorService.LoadSensor(sensorId, A<CancellationToken>._))
            .Returns(Task.FromResult<ApiSensor?>(new ApiSensor { Id = sensorId, Name = sensorName }));

        var services = new ServiceCollection();
        services.AddSingleton(connectorService);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { SourceSensorId = sensorId, Name = "", Event = "all" })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Equal(sensorName, rule.Name);
        Assert.Equal($"Source: {sensorName}", rule.Description);
    }

    [Fact]
    public void GetId_ShouldReturnConsistentGuid()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour1 = new InputRuleBehaviour(serviceProvider);
        var behaviour2 = new InputRuleBehaviour(serviceProvider);

        Assert.Equal(behaviour1.GetId(), behaviour2.GetId());
        Assert.NotEqual(Guid.Empty, behaviour1.GetId());
    }

    [Fact]
    public void DisplayName_ShouldReturnINPUT()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour = new InputRuleBehaviour(serviceProvider);

        Assert.Equal("INPUT", behaviour.DisplayName());
    }

    [Fact]
    public void Inputs_ShouldBeEmpty()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour = new InputRuleBehaviour(serviceProvider);

        Assert.Empty(behaviour.Inputs);
    }

    [Fact]
    public void Outputs_ShouldHaveCorrectConfiguration()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour = new InputRuleBehaviour(serviceProvider);

        Assert.Equal(1, behaviour.Outputs.Min);
        Assert.Equal(1, behaviour.Outputs.Max);
        Assert.Equal(1, behaviour.Outputs.Def);
    }

    [Fact]
    public async Task Execute_WithNonMatchingEventType_ShouldNotSetOutputs()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Outputs = [output],
            ParametersJson = JsonConvert.SerializeObject(new { Event = "TestEvent", SourceSensorId = Guid.NewGuid() })
        };

        var existingEvent = output.Event;
        await behaviour.Execute(rule, new AutomatisationEvent { Type = "DifferentEvent" }, CancellationToken.None);

        Assert.Equal(existingEvent, output.Event);
    }

    [Fact]
    public async Task SetParameters_WithInvalidJson_ShouldThrow()
    {
        var connectorService = A.Fake<IConnectorService>();
        var services = new ServiceCollection();
        services.AddSingleton(connectorService);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var rule = new Rule
        {
            ParametersJson = "invalid json"
        };

        await Assert.ThrowsAsync<JsonReaderException>(async () =>
            await behaviour.SetParameters(rule, CancellationToken.None));
    }

    [Fact]
    public async Task SetParameters_WithNullSensor_ShouldHandleGracefully()
    {
        var sensorId = Guid.NewGuid();
        var connectorService = A.Fake<IConnectorService>();
        A.CallTo(() => connectorService.LoadSensor(sensorId, A<CancellationToken>._))
            .Returns(Task.FromResult<ApiSensor?>(null));

        var services = new ServiceCollection();
        services.AddSingleton(connectorService);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { SourceSensorId = sensorId, Event = "all" })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.NotNull(rule.Name);
    }

    [Fact]
    public async Task Execute_WithNumericValue_ShouldPreserveValue()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Outputs = [output],
            ParametersJson = JsonConvert.SerializeObject(new { Event = "TestEvent", SourceSensorId = Guid.NewGuid() })
        };

        float testValue = 42.5f;
        await behaviour.Execute(rule, new AutomatisationEvent { Type = "TestEvent", NumericValue = testValue }, CancellationToken.None);

        Assert.NotNull(output.Event);
        Assert.Equal(testValue, output.Event!.NumericValue);
    }

    [Fact]
    public async Task Execute_WithStringValue_ShouldPreserveValue()
    {
        var cache = A.Fake<IAutomatizationCache>();
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => cache.GetNextInputs(A<Guid>._)).Returns(Task.FromResult<IEnumerable<InputPointDTO>>([]));

        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton(meterFactory);
        var serviceProvider = services.BuildServiceProvider();

        var behaviour = new InputRuleBehaviour(serviceProvider);
        var output = new OutputPointDTO(Guid.NewGuid(), serviceProvider);
        var rule = new RuleDTO
        {
            Outputs = [output],
            ParametersJson = JsonConvert.SerializeObject(new { Event = "TestEvent", SourceSensorId = Guid.NewGuid() })
        };

        var testString = "Test Value";
        await behaviour.Execute(rule, new AutomatisationEvent { Type = "TestEvent", StringValue = testString }, CancellationToken.None);

        Assert.NotNull(output.Event);
        Assert.Equal(testString, output.Event!.StringValue);
    }
}
