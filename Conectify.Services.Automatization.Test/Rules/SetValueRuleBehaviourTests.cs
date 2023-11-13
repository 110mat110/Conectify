using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

[TestFixture]
public class SetValueRuleBehaviourTests
{
    [Test]
    public void Execute_WhenParametersJsonIsNotNull_ReturnsAutomatisationValueWithStaticValues()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit", TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), SourceId = Guid.NewGuid() }
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Static value"));
        Assert.That(result.NumericValue, Is.EqualTo(10));
        Assert.That(result.StringValue, Is.EqualTo("Test2"));
        Assert.That(result.Unit, Is.EqualTo("Unit2"));
        Assert.That(result.SourceId, Is.EqualTo(masterRule.Id));
    }

    [Test]
    public void Execute_WhenParametersJsonIsNull_ReturnsOriginalAutomatisationValue()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit", TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), SourceId = Guid.NewGuid() }
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
        Assert.That(result, Is.Null);
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Static value"));
        Assert.That(result.NumericValue, Is.EqualTo(10));
        Assert.That(result.StringValue, Is.EqualTo("Test2"));
        Assert.That(result.Unit, Is.EqualTo("Unit2"));
        Assert.That(result.SourceId, Is.EqualTo(rule.Id));
    }

    [Test]
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
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetId_Always_ReturnsCorrectGuid()
    {
        // Arrange
        var setValueRuleBehaviour = new SetValueRuleBehaviour();

        // Act
        var result = setValueRuleBehaviour.GetId();

        // Assert
        Assert.That(result, Is.EqualTo(Guid.Parse("8c173ffc-7243-4675-9a0d-28c2ce19a18f")));
    }

    // Add more tests for different scenarios as needed
}
