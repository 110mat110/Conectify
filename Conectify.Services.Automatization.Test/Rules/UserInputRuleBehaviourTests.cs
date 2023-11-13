using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;

[TestFixture]
public class UserInputRuleBehaviourTests
{
    [Test]
    public void Execute_WhenAutomatisationValuesNotEmpty_ReturnsFirstAutomatisationValue()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit" },
            new AutomatisationValue { NumericValue = 10, StringValue = "Test2", Unit = "Unit2" }
        };

        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var userInputRuleBehaviour = new UserInputRuleBehaviour();

        // Act
        var result = userInputRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NumericValue, Is.EqualTo(5));
        Assert.That(result.StringValue, Is.EqualTo("Test"));
        Assert.That(result.Unit, Is.EqualTo("Unit"));
    }

    [Test]
    public void Execute_WhenAutomatisationValuesEmpty_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>();
        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var userInputRuleBehaviour = new UserInputRuleBehaviour();

        // Act
        var result = userInputRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var userInputRuleBehaviour = new UserInputRuleBehaviour();

        // Act
        var result = userInputRuleBehaviour.GetId();

        // Assert
        Assert.That(result, Is.EqualTo(Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925798")));
    }

    [Test]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var userInputRuleBehaviour = new UserInputRuleBehaviour();

        // Act
        var result = userInputRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.That(result, Is.Null);
    }

    // Add more tests for different scenarios as needed
}
