using Conectify.Services.Automatization.Models;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class SetValueRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        return InitializationValue(masterRule);
    }

    public AutomatisationValue? InitializationValue(RuleDTO rule)
    {
        if (string.IsNullOrEmpty(rule.ParametersJson))
        {
            return null;
        }

        var value = JsonConvert.DeserializeObject<SetValueOptions>(rule.ParametersJson);

        if (value is null)
        {
            return null;
        }

        return new AutomatisationValue()
        {
            Name = "Static value",
            NumericValue = value.NumericValue,
            StringValue = value.StringValue,
            Unit = value.Unit,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = rule.Id,
        };
    }

    public string DisplayName() => "SET VALUE";
    public Guid GetId()
    {
        return Guid.Parse("8c173ffc-7243-4675-9a0d-28c2ce19a18f");
    }

    private record SetValueOptions
    {
        public string StringValue = string.Empty;
        public float NumericValue = 0;
        public string Unit = string.Empty;
    }
}
