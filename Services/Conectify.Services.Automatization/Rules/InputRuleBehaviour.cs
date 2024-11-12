using Conectify.Services.Automatization.Models;
using Conectify.Shared.Library;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class InputRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        Options = JsonConvert.DeserializeObject<InputRuleOptions>(masterRule.ParametersJson);

        return automatisationValues.FirstOrDefault(x => string.IsNullOrEmpty(Options?.Event) || Options.Event == Constants.Events.All || x.Type == Options.Event);
    }

    public string DisplayName() => "ON EVENT";

    public Guid GetId()
    {
        return Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925797");
    }

    public AutomatisationValue? InitializationValue(RuleDTO rule)
    {
        return null;
    }

    internal InputRuleOptions? Options { get; set; }
}

internal class InputRuleOptions
{
    public string Event { get; set; } = string.Empty;
}