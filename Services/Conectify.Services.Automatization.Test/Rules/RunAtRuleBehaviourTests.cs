using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Rules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

public class RunAtRuleBehaviourTests
{
    [Fact]
    public void GetId_ShouldReturnCorrectGuid()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behaviour = new RunAtRuleBehaviour(serviceProvider);

        var expectedId = Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a");
        var actualId = behaviour.GetId();

        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void DisplayName_ShouldReturnRunAt()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behaviour = new RunAtRuleBehaviour(serviceProvider);

        var displayName = behaviour.DisplayName();

        Assert.Equal("RUN AT", displayName);
    }

    [Fact]
    public void Outputs_ShouldHaveCorrectConfiguration()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behaviour = new RunAtRuleBehaviour(serviceProvider);

        var outputs = behaviour.Outputs;

        Assert.Equal(1, outputs.Min);
        Assert.Equal(1, outputs.Def);
        Assert.Equal(1, outputs.Max);
    }

    [Fact]
    public void Inputs_ShouldAllowNoInputs()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behaviour = new RunAtRuleBehaviour(serviceProvider);

        var inputs = behaviour.Inputs.ToList();

        Assert.All(inputs, input =>
        {
            Assert.Equal(0, input.Item2.Min);
            Assert.Equal(0, input.Item2.Def);
            Assert.Equal(0, input.Item2.Max);
        });
    }

    [Fact]
    public async Task Execute_ShouldCompleteSuccessfully()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behaviour = new RunAtRuleBehaviour(serviceProvider);

        var ruleDTO = A.Fake<RuleDTO>();

        await behaviour.Execute(ruleDTO, new AutomatisationEvent());

        Assert.True(true);
    }

    [Fact]
    public async Task SetParameters_WithTimeAndDays_ShouldSetNameAndDescription()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behaviour = new RunAtRuleBehaviour(serviceProvider);

        var timeSet = new DateTime(2024, 1, 1, 14, 30, 0, DateTimeKind.Utc);
        var days = "Mo,We,Fr";
        var parametersJson = JsonConvert.SerializeObject(new
        {
            TimeSet = timeSet,
            Days = days
        });

        var rule = new Rule
        {
            Id = Guid.NewGuid(),
            ParametersJson = parametersJson
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains(days, rule.Name);
        Assert.Contains(days, rule.Description);
        Assert.Contains("Run at", rule.Name);
        Assert.Contains("Run at", rule.Description);
    }

    [Fact]
    public async Task InitializationValue_ShouldCompleteSuccessfully()
    {
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behaviour = new RunAtRuleBehaviour(serviceProvider);

        var ruleDTO = A.Fake<RuleDTO>();

        await behaviour.InitializationValue(ruleDTO, null);

        Assert.True(true);
    }
}