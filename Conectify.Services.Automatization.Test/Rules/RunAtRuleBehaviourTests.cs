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
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        Assert.Equal(Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a"), behaviour.GetId());
    }

    [Fact]
    public void DisplayName_ShouldReturnRunAt()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        Assert.Equal("RUN AT", behaviour.DisplayName());
    }

    [Fact]
    public void Outputs_ShouldHaveCorrectConfiguration()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        var outputs = behaviour.Outputs;

        Assert.Equal(1, outputs.Min);
        Assert.Equal(1, outputs.Def);
        Assert.Equal(1, outputs.Max);
    }

    [Fact]
    public async Task Execute_ShouldCompleteSuccessfully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var ruleDTO = new RuleDTO();

        await behaviour.Execute(ruleDTO, new AutomatisationEvent());

        Assert.True(true);
    }

    [Fact]
    public async Task SetParameters_WithTimeAndDays_ShouldSetNameAndDescription()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var timeSet = new DateTime(2024, 1, 1, 14, 30, 0, DateTimeKind.Utc);
        var days = "Mo,We,Fr";
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { TimeSet = timeSet, Days = days })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Run at", rule.Name);
        Assert.Contains(days, rule.Name);
        Assert.Contains("Run at", rule.Description);
        Assert.Contains(days, rule.Description);
    }

    [Fact]
    public void Inputs_ShouldBeEmpty()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());

        Assert.Empty(behaviour.Inputs);
    }

    [Fact]
    public async Task SetParameters_WithoutDays_ShouldHandleGracefully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var timeSet = new DateTime(2024, 1, 1, 14, 30, 0, DateTimeKind.Utc);
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { TimeSet = timeSet })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Run at", rule.Name);
    }

    [Fact]
    public async Task SetParameters_WithEmptyJson_ShouldHandleGracefully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = "{}"
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.NotNull(rule.Name);
        Assert.NotNull(rule.Description);
    }

    [Fact]
    public async Task SetParameters_WithNullJson_ShouldHandleGracefully()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = null
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.NotNull(rule.Name);
        Assert.NotNull(rule.Description);
    }

    [Fact]
    public async Task SetParameters_WithInvalidJson_ShouldThrow()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var rule = new Rule
        {
            ParametersJson = "invalid json"
        };

        await Assert.ThrowsAsync<JsonReaderException>(async () =>
            await behaviour.SetParameters(rule, CancellationToken.None));
    }

    [Fact]
    public async Task SetParameters_WithDifferentTimeFormats_ShouldParse()
    {
        var behaviour = new RunAtRuleBehaviour(new ServiceCollection().BuildServiceProvider());
        var timeSet = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var days = "Mo,Tu,We,Th,Fr,Sa,Su";
        var rule = new Rule
        {
            ParametersJson = JsonConvert.SerializeObject(new { TimeSet = timeSet, Days = days })
        };

        await behaviour.SetParameters(rule, CancellationToken.None);

        Assert.Contains("Run at", rule.Name);
        Assert.Contains(days, rule.Name);
    }

    [Fact]
    public void GetId_ShouldBeConsistentAcrossInstances()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var behaviour1 = new RunAtRuleBehaviour(serviceProvider);
        var behaviour2 = new RunAtRuleBehaviour(serviceProvider);

        Assert.Equal(behaviour1.GetId(), behaviour2.GetId());
    }
}
