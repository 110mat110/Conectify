using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

public class TimeRuleBehaviourTests
{
    [Fact]
    public void Execute_WhenTriggerTimeIsNow_ReturnsAutomatisationValueWithTrueValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationEvent>();
        var triggerTime = DateTime.UtcNow;
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { TriggerTime = triggerTime }),
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationEvent>>();

        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TimeRuleResult", result.Name);
        Assert.Equal(result.NumericValue, 1);
        Assert.Equal("true", result.StringValue);
        Assert.Equal("", result.Unit);
        Assert.True(result.TimeCreated.CompareTo( DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) >= 0);
        Assert.Equal(result.SourceId, masterRule.Id);
    }

    [Fact]
    public void Execute_WhenTriggerTimeIsNotNow_ReturnsAutomatisationValueWithFalseValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationEvent>();
        var triggerTime = DateTime.UtcNow.AddMinutes(1); // Future time
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { TriggerTime = triggerTime }),
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationEvent>>();

        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TimeRuleResult", result.Name);
        Assert.Equal(result.NumericValue, 0);
        Assert.Equal("false", result.StringValue);
        Assert.Equal("", result.Unit);
        Assert.True(result.TimeCreated.CompareTo(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())>=0);
        Assert.Equal(result.SourceId, masterRule.Id);
    }

    [Fact]
    public void Execute_WhenOptionsIsNull_ReturnsAutomatisationValueWithFalseValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationEvent>();
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = null,
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationEvent>>();

        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.GetId();

        // Assert
        Assert.Equal(result, Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a"));
    }

    [Fact]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.Null(result);
    }

    // Add more tests for different scenarios as needed
}
