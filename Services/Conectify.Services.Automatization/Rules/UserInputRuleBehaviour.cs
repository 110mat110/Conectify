using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class UserInputRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        return automatisationValues.FirstOrDefault();
    }

    public AutomatisationValue? InitializationValue(RuleDTO rule)
    {
        return null;
    }

    public string DisplayName() => "USER INPUT";

    public Guid GetId()
    {
        return Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925798");
    }
}
