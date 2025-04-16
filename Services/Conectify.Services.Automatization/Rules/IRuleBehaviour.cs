using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;

namespace Conectify.Services.Automatization.Rules;

public interface IRuleBehaviour
{
    Guid GetId();

    string DisplayName();

    MinMaxDef Outputs { get; }

    IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs { get; }

    Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default);

    Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO);

    void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default);

    Task SetParameters(Rule rule, CancellationToken cancellationToken);
}
