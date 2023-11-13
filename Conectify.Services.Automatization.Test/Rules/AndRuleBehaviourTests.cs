using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;

[TestFixture]
public class AndRuleBehaviourTests
{


    [SetUp]
    public void SetUp()
    {

    }

    [Test]
    public void Execute_WhenAllNumericValuesNonZero_ReturnsAutomatisationValueWithNumericValue1()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
    {
        new AutomatisationValue { NumericValue = 2 },
        new AutomatisationValue { NumericValue = 3 }
        // Add more AutomatisationValue objects as needed
    };

        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var andRuleBehaviour = new AndRuleBehaviour();

        // Act
        var result = andRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("AndRuleResult"));
        Assert.That(result.NumericValue, Is.EqualTo(1));
        Assert.That(result.StringValue, Is.EqualTo("true"));
        Assert.That(result.Unit, Is.EqualTo(""));
        Assert.That(result.SourceId, Is.EqualTo(masterRule.Id));
    }

    [Test]
    public void Execute_WhenAnyNumericValueIsZero_ReturnsAutomatisationValueWithNumericValue0()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
    {
        new AutomatisationValue { NumericValue = 2 },
        new AutomatisationValue { NumericValue = 0 }
        // Add more AutomatisationValue objects as needed
    };

        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var andRuleBehaviour = new AndRuleBehaviour();

        // Act
        var result = andRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("AndRuleResult"));
        Assert.That(result.NumericValue, Is.EqualTo(0));
        Assert.That(result.StringValue, Is.EqualTo("false"));
        Assert.That(result.Unit, Is.EqualTo(""));
        Assert.That(result.SourceId, Is.EqualTo(masterRule.Id));
    }

    // Add more tests for other scenarios as needed
}
