using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class InputRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        return automatisationValues.FirstOrDefault();
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
}
