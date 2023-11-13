using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

[TestFixture]
public class TimeRuleBehaviourTests
{
    [Test]
    public void Execute_WhenTriggerTimeIsNow_ReturnsAutomatisationValueWithTrueValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>();
        var triggerTime = DateTime.UtcNow;
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { TriggerTime = triggerTime }),
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("TimeRuleResult"));
        Assert.That(result.NumericValue, Is.EqualTo(1));
        Assert.That(result.StringValue, Is.EqualTo("true"));
        Assert.That(result.Unit, Is.EqualTo(""));
        Assert.GreaterOrEqual(result.TimeCreated, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        Assert.That(result.SourceId, Is.EqualTo(masterRule.Id));
    }

    [Test]
    public void Execute_WhenTriggerTimeIsNotNow_ReturnsAutomatisationValueWithFalseValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>();
        var triggerTime = DateTime.UtcNow.AddMinutes(1); // Future time
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { TriggerTime = triggerTime }),
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("TimeRuleResult"));
        Assert.That(result.NumericValue, Is.EqualTo(0));
        Assert.That(result.StringValue, Is.EqualTo("false"));
        Assert.That(result.Unit, Is.EqualTo(""));
        Assert.GreaterOrEqual(result.TimeCreated, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        Assert.That(result.SourceId, Is.EqualTo(masterRule.Id));
    }

    [Test]
    public void Execute_WhenOptionsIsNull_ReturnsAutomatisationValueWithFalseValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>();
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = null,
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.GetId();

        // Assert
        Assert.That(result, Is.EqualTo(Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a")));
    }

    [Test]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var timeRuleBehaviour = new TimeRuleBehaviour();

        // Act
        var result = timeRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.That(result, Is.Null);
    }

    // Add more tests for different scenarios as needed
}
