using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class AndRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        return new AutomatisationValue()
        {
            Name = "AndRuleResult",
            NumericValue = !automatisationValues.Any(x => x.NumericValue == 0) ? 1 : 0,
            StringValue = !automatisationValues.Any(x => x.NumericValue == 0) ? "true" :
            "false",
            Unit = "",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = masterRule.Id
        };

    }

    public string DisplayName() => "AND Rule";

    public Guid GetId()
    {
        return Guid.Parse("28ff4530-887b-48d1-a4fa-38dc839257a4");
    }

    public AutomatisationValue? InitializationValue(RuleDTO rule)
    {
        return new AutomatisationValue()
        {
            Name = "AndRuleResult",
            NumericValue = 0,
            StringValue = "false",
            Unit = "",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = rule.Id
        };
    }
}
