using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class InputRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue Execute(IEnumerable<AutomatisationValue> automationValues, RuleDTO ruleDTO)
    {
        return automationValues.First();
    }

    public Guid GetId()
    {
        return Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925797");
    }
}
