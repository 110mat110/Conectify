using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;

public class RunAtRuleBehaviourTests
{
    [Fact]
    public void Execute_Always_ReturnsAutomatisationValueWithCorrectValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationEvent>();
        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationEvent>>();

        var runAtRuleBehaviour = new RunAtRuleBehaviour();

        // Act
        var result = runAtRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TimeRuleResult", result.Name);
        Assert.Equal(result.NumericValue, 0);
        Assert.Equal("", result.Unit);
        Assert.Equal(result.SourceId, masterRule.Id);
        Assert.True(result.TimeCreated.CompareTo(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) >= 0);
    }

    [Fact]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var runAtRuleBehaviour = new RunAtRuleBehaviour();

        // Act
        var result = runAtRuleBehaviour.GetId();

        // Assert
        Assert.Equal(result, Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a"));
    }

    [Fact]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var runAtRuleBehaviour = new RunAtRuleBehaviour();

        // Act
        var result = runAtRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.Null(result);
    }

    // Add more tests for different scenarios as needed
}
