using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class AndRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule)
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

    public Guid GetId()
    {
        return Guid.Parse("28ff4530-887b-48d1-a4fa-38dc839257a4");
    }
}
