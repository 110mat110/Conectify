using Conectify.Services.Automatization.Models;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class RunAtRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule)
    {
        var options = JsonConvert.DeserializeObject<TimeRuleOptions>(masterRule.ParametersJson);

        var isNow = false;

        if (options != null && DateTime.UtcNow.Subtract(options.TriggerTime).TotalSeconds < 1)
        {
            isNow = true;
        }

        return new AutomatisationValue()
        {
            Name = "TimeRuleResult",
            NumericValue = isNow ? 1 : 0,
            StringValue = isNow ? "true" : "false",
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
        public DateTime TriggerTime { get; set; }
    }
}
