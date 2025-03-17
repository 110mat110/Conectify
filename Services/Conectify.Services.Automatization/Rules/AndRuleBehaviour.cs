using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;

namespace Conectify.Services.Automatization.Rules;

public class AndRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public MinMaxDef Outputs => new MinMaxDef(1,1,1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => new List<Tuple<InputTypeEnum, MinMaxDef>>() { 
            new(InputTypeEnum.Value, new(2,2,10)), 
            new(InputTypeEnum.Trigger, new(0,1,1)), 
            new(InputTypeEnum.Parameter, new(0,0,0)) 
        };

    public async Task Execute(RuleDTO masterRule, AutomatisationEvent trigger, CancellationToken ct)
    {
        var inputs = masterRule.Inputs.Select( x => x.GetEvent(serviceProvider).Result);
        var outputValue = new AutomatisationEvent()
        {
            Name = "AndRuleResult",
            NumericValue = !inputs.Any( x => x is not null && x.NumericValue == 0) ? 1 : 0,
            StringValue = !inputs.Any(x => x is not null && x.NumericValue == 0) ? "true" : "false",
            Unit = "",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = masterRule.Id
        };

        await masterRule.SetAllOutputs(outputValue);

        return;
    }

    public string DisplayName() => "AND Rule";

    public Guid GetId()
    {
        return Guid.Parse("28ff4530-887b-48d1-a4fa-38dc839257a4");
    }

    public async Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        if (await rule.SetAllOutputs(oldDTO))
        {
            return;
        }

        var outputValue = new AutomatisationEvent()
        {
            Name = "AndRuleResult",
            NumericValue = 0,
            StringValue = "false",
            Unit = "",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = rule.Id
        };

        await rule.SetAllOutputs(outputValue, false);
        return;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    { 
    }

    public async Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        rule.Description = "AND";
    }
}
