using Conectify.Services.Automatization.Models;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class TimeRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        if (string.IsNullOrEmpty(masterRule.ParametersJson))
        {
            return null;
        }

        var options = JsonConvert.DeserializeObject<TimeRuleOptions>(masterRule.ParametersJson);

        var isNow = false;

        if (options != null && DateTime.UtcNow.Subtract(options.TriggerTime).TotalSeconds > 0)
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

    public string DisplayName() => "TIME ELAPSED";

    public Guid GetId()
    {
        return Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a");
    }

    public AutomatisationValue? InitializationValue(RuleDTO rule)
    {
        return null;
    }

    private class TimeRuleOptions
    {
        public DateTime TriggerTime { get; set; }
    }
}
