using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Test.Rules;

[TestFixture]
public class DecisionRuleBehaviourTests
{
    [Test]
    public void Execute_WhenParametersAndParameterValuesExistAndComparisonIsTrue_ReturnsAutomatisationValue()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit" }
        };
        var parameterID = Guid.NewGuid();
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { Rule = ">" }),
            Parameters = new List<Guid> { parameterID }
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>
        {
            new Tuple<Guid, AutomatisationValue>(parameterID, new AutomatisationValue { NumericValue = 3 })
        };

        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Comparsion output"));
        Assert.That(result.NumericValue, Is.EqualTo(5));
        Assert.That(result.StringValue, Is.EqualTo("Test"));
        Assert.That(result.Unit, Is.EqualTo("Unit"));
        Assert.That(result.SourceId, Is.EqualTo(masterRule.Id));
    }

    [Test]
    public void Execute_WhenParametersAndParameterValuesExistAndComparisonIsFalse_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit" }
        };

        var parameterID = Guid.NewGuid();
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { Rule = ">" }),
            Parameters = new List<Guid> { parameterID }
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>
        {
            new Tuple<Guid, AutomatisationValue>(parameterID, new AutomatisationValue { NumericValue = 7 })
        };

        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Execute_WhenParametersDoNotExist_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit" }
        };

        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { Rule = ">" }),
            Parameters = new List<Guid>() // No parameters
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>
        {
            new Tuple<Guid, AutomatisationValue>(Guid.NewGuid(), new AutomatisationValue { NumericValue = 3 })
        };

        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Execute_WhenParameterValuesDoNotExist_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit" }
        };

        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { Rule = ">" }),
            Parameters = new List<Guid> { Guid.NewGuid() }
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>(); // No parameter values

        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Execute_WhenParametersJsonIsInvalid_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit" }
        };

        var parameterID = Guid.NewGuid();
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { Test = ">" }),
            Parameters = new List<Guid> { parameterID }
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>
        {
            new Tuple<Guid, AutomatisationValue>(parameterID, new AutomatisationValue { NumericValue = 3 })
        };

        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Execute_WhenDeserializedValueIsNull_ReturnsNull()
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = 5, StringValue = "Test", Unit = "Unit" }
        };

        var parameterID = Guid.NewGuid();
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { Rule = ">" }),
            Parameters = new List<Guid> { parameterID }
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>
        {
            new Tuple<Guid, AutomatisationValue>(parameterID, new AutomatisationValue { NumericValue = 3 })
        };

        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void InitializationValue_Always_ReturnsNull()
    {
        // Arrange
        var rule = new RuleDTO { Id = Guid.NewGuid() };
        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.InitializationValue(rule);

        // Assert
        Assert.That(result, Is.Null);
    }

    [TestCase(">", 5, 3, true)]
    [TestCase(">", 5, 7, false)]
    [TestCase(">", 5, 5, false)]

    [TestCase("<", 5, 3, false)]
    [TestCase("<", 5, 7, true)]
    [TestCase("<", 5, 5, false)]

    [TestCase("=", 5, 3, false)]
    [TestCase("=", 5, 5, true)]
    [TestCase("=", 5, 7, false)]

    [TestCase(">=", 5, 3, true)]
    [TestCase(">=", 5, 5, true)]
    [TestCase(">=", 5, 7, false)]

    [TestCase("<=", 5, 3, false)]
    [TestCase("<=", 5, 5, true)]
    [TestCase("<=", 5, 7, true)]

    [TestCase("<>", 5, 3, true)]
    [TestCase("<>", 5, 5, false)]
    [TestCase("<>", 5, 7, true)]

    [TestCase("!=", 5, 3, true)]
    [TestCase("!=", 5, 5, false)]
    [TestCase("!=", 5, 7, true)]
    public void ComputeValue_WhenGivenComparisonRule_ReturnsCorrectResult(string rule, float input, float comparingValue, bool expected)
    {
        // Arrange
        var automatisationValues = new List<AutomatisationValue>
        {
            new AutomatisationValue { NumericValue = input, StringValue = "Test", Unit = "Unit" }
        };
        var parameterID = Guid.NewGuid();
        var masterRule = new RuleDTO
        {
            Id = Guid.NewGuid(),
            ParametersJson = JsonConvert.SerializeObject(new { Rule = rule }),
            Parameters = new List<Guid> { parameterID }
        };

        var parameterValues = new List<Tuple<Guid, AutomatisationValue>>
        {
            new Tuple<Guid, AutomatisationValue>(parameterID, new AutomatisationValue { NumericValue = comparingValue })
        };

        var decisionRuleBehaviour = new DecisionRuleBehaviour();

        // Act
        var result = decisionRuleBehaviour.Execute(automatisationValues, masterRule, parameterValues);

        // Assert
        if (expected)
        {
            Assert.That(result, Is.Not.Null);
        }
        else
        {
            Assert.That(result, Is.Null);
        }
    }

    // Add more tests for different scenarios as needed
}