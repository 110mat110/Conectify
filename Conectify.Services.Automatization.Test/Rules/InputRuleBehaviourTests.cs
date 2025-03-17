using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;

namespace Conectify.Services.Automatization.Test.Rules;

public class InputRuleBehaviourTests
{
    [Fact]
    public void Execute_WhenAutomatisationValuesNotEmpty_ReturnsFirstAutomatisationValue()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationEvent>
            {
                new() { NumericValue = 5, StringValue = "Test", Unit = "Unit" },
                new() { NumericValue = 10, StringValue = "Test2", Unit = "Unit2" }
            };

        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationEvent>>();

        var inputRuleBehaviour = new InputRuleBehaviour();

        // Act
        var result = inputRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.NumericValue, 5);
        Assert.Equal("Test", result.StringValue);
        Assert.Equal("Unit", result.Unit);
    }

    [Fact]
    public void Execute_WhenAutomatisationValuesEmpty_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationEvent>();
        var masterRule = new RuleDTO { Id = Guid.NewGuid() };
        var parameterValues = new List<Tuple<Guid, AutomatisationEvent>>();

        var inputRuleBehaviour = new InputRuleBehaviour();

        // Act
        var result = inputRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var inputRuleBehaviour = new InputRuleBehaviour();

        // Act
        var result = inputRuleBehaviour.GetId();

        // Assert
        Assert.Equal(result, Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925797"));
    }

    [Fact]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var inputRuleBehaviour = new InputRuleBehaviour();

        // Act
        var result = inputRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.Null(result);
    }

    // Add more tests for different scenarios as needed
}
