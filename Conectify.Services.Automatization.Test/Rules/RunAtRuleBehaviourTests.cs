using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;

[TestFixture]
public class RunAtRuleBehaviourTests
{
    [Test]
    public void Execute_Always_ReturnsAutomatisationValueWithCorrectValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>();
        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var runAtRuleBehaviour = new RunAtRuleBehaviour();

        // Act
        var result = runAtRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("TimeRuleResult"));
        Assert.That(result.NumericValue, Is.EqualTo(0));
        Assert.That(result.Unit, Is.EqualTo(""));
        Assert.That(result.SourceId, Is.EqualTo(masterRule.Id));
        Assert.GreaterOrEqual(result.TimeCreated, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    [Test]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var runAtRuleBehaviour = new RunAtRuleBehaviour();

        // Act
        var result = runAtRuleBehaviour.GetId();

        // Assert
        Assert.That(result, Is.EqualTo(Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a")));
    }

    [Test]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var runAtRuleBehaviour = new RunAtRuleBehaviour();

        // Act
        var result = runAtRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.That(result, Is.Null);
    }

    // Add more tests for different scenarios as needed
}
