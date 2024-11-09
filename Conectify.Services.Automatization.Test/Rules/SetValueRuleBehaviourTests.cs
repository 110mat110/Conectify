using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

public class SetValueRuleBehaviourTests
{
    [Fact]
    public void Execute_WhenParametersJsonIsNotNull_ReturnsAutomatisationValueWithStaticValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new() { NumericValue = 5, StringValue = "Test", Unit = "Unit", TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), SourceId = Guid.NewGuid() }
        };

        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { NumericValue = 10, StringValue = "Test2", Unit = "Unit2" }),
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var setValueRuleBehaviour = new SetValueRuleBehaviour();

        // Act
        var result = setValueRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Static value", result.Name);
        Assert.Equal(result.NumericValue, 10);
        Assert.Equal("Test2", result.StringValue);
        Assert.Equal("Unit2", result.Unit);
        Assert.Equal(result.SourceId, masterRule.Id);
    }

    [Fact]
    public void Execute_WhenParametersJsonIsNull_ReturnsOriginalAutomatisationValue()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new() { NumericValue = 5, StringValue = "Test", Unit = "Unit", TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), SourceId = Guid.NewGuid() }
        };

        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = null,
            Parameters = new List<Guid>()
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>();

        var setValueRuleBehaviour = new SetValueRuleBehaviour();

        // Act
        var result = setValueRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void InitializationValue_WhenParametersJsonIsNotNull_ReturnsAutomatisationValueWithStaticValues()
    {
        // Arrange
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { NumericValue = 10, StringValue = "Test2", Unit = "Unit2" }),
            Parameters = new List<Guid>()
        };

        var setValueRuleBehaviour = new SetValueRuleBehaviour();

        // Act
        var result = setValueRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Static value", result.Name);
        Assert.Equal(result.NumericValue, 10);
        Assert.Equal("Test2", result.StringValue);
        Assert.Equal("Unit2", result.Unit);
        Assert.Equal(result.SourceId, rule.Id);
    }

    [Fact]
    public void InitializationValue_WhenParametersJsonIsNull_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = null,
            Parameters = new List<Guid>()
        };

        var setValueRuleBehaviour = new SetValueRuleBehaviour();

        // Act
        var result = setValueRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var setValueRuleBehaviour = new SetValueRuleBehaviour();

        // Act
        var result = setValueRuleBehaviour.GetId();

        // Assert
        Assert.Equal(result, Guid.Parse("8c173ffc-7243-4675-9a0d-28c2ce19a18f"));
    }

    // Add more tests for different scenarios as needed
}
