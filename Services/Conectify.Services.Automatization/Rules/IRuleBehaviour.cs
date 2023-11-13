﻿using Conectify.Services.Automatization.Models;

namespace Conectify.Services.Automatization.Rules;

public interface IRuleBehaviour
{
    Guid GetId();
    AutomatisationValue? Execute(IEnumerable<AutomatisationValue> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid,AutomatisationValue>> parameterValues);

    AutomatisationValue? InitializationValue(RuleDTO rule);
}
