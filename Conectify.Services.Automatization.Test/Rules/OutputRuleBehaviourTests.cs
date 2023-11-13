using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;
[TestFixture]
public class OutputRuleBehaviourTests
{
    [Test]
    public void Execute_WhenAutomatisationValuesNotEmpty_ReturnsLastAutomatisationValueWithDestinationIdSet()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit", TimeCreated = 1 },
            new AutomatisationValue { NumericValue = 10, StringValue = "Test2", Unit = "Unit2", TimeCreated = 2 }
        };

        var masterRule = new RuleDTO { Id = Guid.NewGuid(), DestinationActuatorId = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var outputRuleBehaviour = new OutputRuleBehaviour();

        // Act
        var result = outputRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NumericValue, Is.EqualTo(10));
        Assert.That(result.StringValue, Is.EqualTo("Test2"));
        Assert.That(result.Unit, Is.EqualTo("Unit2"));
        Assert.That(result.DestinationId, Is.EqualTo(masterRule.DestinationActuatorId));
    }

    [Test]
    public void Execute_WhenAutomatisationValuesEmpty_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>();
        var masterRule = new RuleDTO { Id = Guid.NewGuid(), DestinationActuatorId = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var outputRuleBehaviour = new OutputRuleBehaviour();

        // Act
        var result = outputRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var outputRuleBehaviour = new OutputRuleBehaviour();

        // Act
        var result = outputRuleBehaviour.GetId();

        // Assert
        Assert.That(result, Is.EqualTo(Guid.Parse("d274c7f0-211e-413a-8689-f2543dbfc818")));
    }

    [Test]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var outputRuleBehaviour = new OutputRuleBehaviour();

        // Act
        var result = outputRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.That(result, Is.Null);
    }

    // Add more tests for different scenarios as needed
}
