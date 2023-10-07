using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class RunAtRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule)
    {
        return new AutomatisationValue()
        {
            Name = "TimeRuleResult",
            NumericValue = 0,
            Unit = "",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = masterRule.Id
        };
    }

    public Guid GetId()
    {
        return Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a");
    }

    private class TimeRuleOptions
    {
        public DateTime TimeSet { get; set; }
    }
}
