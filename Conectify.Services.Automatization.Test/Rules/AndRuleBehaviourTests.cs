using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;

public class AndRuleBehaviourTests
{
    public void SetUp()
    {

    }

    [Fact]
    public void Execute_WhenAllNumericValuesNonZero_ReturnsAutomatisationValueWithNumericValue1()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
    {
        new() { NumericValue = 2 },
        new() { NumericValue = 3 }
        // Add more AutomatisationValue objects as needed
    };

        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var andRuleBehaviour = new AndRuleBehaviour();

        // Act
        var result = andRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AndRuleResult", result.Name);
        Assert.Equal(result.NumericValue, 1);
        Assert.Equal("true", result.StringValue);
        Assert.Equal("", result.Unit);
        Assert.Equal(result.SourceId, masterRule.Id);
    }

    [Fact]
    public void Execute_WhenAnyNumericValueIsZero_ReturnsAutomatisationValueWithNumericValue0()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
    {
        new() { NumericValue = 2 },
        new() { NumericValue = 0 }
        // Add more AutomatisationValue objects as needed
    };

        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var andRuleBehaviour = new AndRuleBehaviour();

        // Act
        var result = andRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AndRuleResult", result.Name);
        Assert.Equal(result.NumericValue, 0);
        Assert.Equal("false", result.StringValue);
        Assert.Equal("", result.Unit);
        Assert.Equal(result.SourceId, masterRule.Id);
    }

    // Add more tests for other scenarios as needed
}
