using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;

namespace Conectify.Services.Automatization.Rules;

public class UserInputRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public int DefaultOutputs => 1;

    public IEnumerable<Tuple<InputTypeEnum, int>> DefaultInputs => [new(InputTypeEnum.Value, 0), new(InputTypeEnum.Trigger, 0)];


    public AutomatisationEvent? Execute(IEnumerable<AutomatisationEvent> automatisationValues, RuleDTO masterRule, IEnumerable<Tuple<Guid, AutomatisationEvent>> parameterValues)
    {
        return automatisationValues.FirstOrDefault();
    }

    public AutomatisationEvent? InitializationValue(RuleDTO rule)
    {
        return null;
    }

    public string DisplayName() => "USER INPUT";

    public Guid GetId()
    {
        return Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925798");
    }

    public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    Task IRuleBehaviour.InitializationValue(RuleDTO rule)
    {
        throw new NotImplementedException();
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
