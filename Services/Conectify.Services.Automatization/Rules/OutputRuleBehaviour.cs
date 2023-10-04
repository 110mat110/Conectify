using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class OutputRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue Execute(IEnumerable<AutomatisationValue> automationValues, RuleDTO rule)
    {
        var value = automationValues.OrderBy(x => x.TimeCreated).Last();
        value.DestinationId = rule.DestinationActuatorId;
        return value;
    }

    public Guid GetId()
    {
        return Guid.Parse("d274c7f0-211e-413a-8689-f2543dbfc818");
    }
}
