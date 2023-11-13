using Conectify.Services.Automatization.Models;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class DecisionRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        if (!masterRule.Parameters.Any() || !parameterValues.Any())
        {
            return null;
        }

        var value = JsonConvert.DeserializeObject<DecisionOptions>(masterRule.ParametersJson);

        if(value is null) {
            return null;
        }

        var input = automatisationValues.First();
        var param = parameterValues.First();

        if(ComputeValue(value.Rule, input.NumericValue, param.Item2.NumericValue))
        {
            return new AutomatisationValue()
            {
                Name = "Comparsion output",
                NumericValue = input.NumericValue,
                StringValue = input.StringValue,
                Unit = input.Unit,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SourceId = masterRule.Id
            };
        }

        return null;
    }

    public Guid GetId()
    {
        return Guid.Parse("62d50548-fff0-44c4-8bf3-b592042b1c2b");
    }

    public AutomatisationValue? InitializationValue(RuleDTO rule)
    {
        return null;
    }

    private bool ComputeValue(string param, float? input, float? comparingValue)
    {
        return param switch
        {
            ">" => input > comparingValue,
            "<" => input < comparingValue,
            "=" => input == comparingValue,
            ">=" => input >= comparingValue,
            "<=" => input <= comparingValue,
            "<>" => input != comparingValue,
            "!=" => input != comparingValue,
            _ => false,
        };
    }

    private record DecisionOptions
    {
        public string Rule = string.Empty;
    }
}
