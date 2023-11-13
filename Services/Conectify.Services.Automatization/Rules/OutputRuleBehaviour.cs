using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public class OutputRuleBehaviour : IRuleBehaviour
{
    public AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationValue>> parameterValues)
    {
        if (!automatisationValues.Any())
        {
            return null;
        }

        var value = automatisationValues.OrderBy(x => x.TimeCreated).Last();
        value.DestinationId = masterRule.DestinationActuatorId;
        return value;
    }

    public Guid GetId()
    {
        return Guid.Parse("d274c7f0-211e-413a-8689-f2543dbfc818");
    }
    public AutomatisationValue? InitializationValue(RuleDTO rule)
    {
        return null;
    }
}
