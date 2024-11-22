using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;

namespace Conectify.Services.Automatization.Rules;

public interface IRuleBehaviour
{
    Guid GetId();

    string DisplayName();

    int DefaultOutputs { get; }

    IEnumerable<Tuple<InputTypeEnum, int>> DefaultInputs { get;}

    Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default);

    Task InitializationValue(RuleDTO rule);

    void Clock(RuleDTO masterRule, TimeSpan interval,  CancellationToken ct = default);
}
