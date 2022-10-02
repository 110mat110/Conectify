using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class OutputRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue Execute(IEnumerable<AutomatisationValue> automationValues, RuleDTO rule)
    {
        return automationValues.First();
    }

    public Guid GetId()
    {
        return Guid.Parse("f5bd9cac215f4d2887cf959aa9cdf74c");
    }
}
