using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;

public class OutputRuleBehaviourTests
{
    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(result.NumericValue, 10);
        Assert.Equal("Test2", result.StringValue);
        Assert.Equal("Unit2", result.Unit);
        Assert.Equal(result.DestinationId, masterRule.DestinationActuatorId);
    }

    [Fact]
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
        Assert.Null(result);
    }

    [Fact]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var outputRuleBehaviour = new OutputRuleBehaviour();

        // Act
        var result = outputRuleBehaviour.GetId();

        // Assert
        Assert.Equal(result, Guid.Parse("d274c7f0-211e-413a-8689-f2543dbfc818"));
    }

    [Fact]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var outputRuleBehaviour = new OutputRuleBehaviour();

        // Act
        var result = outputRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.Null(result);
    }

    // Add more tests for different scenarios as needed
}
