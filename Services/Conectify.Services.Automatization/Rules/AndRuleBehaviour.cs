using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;

namespace Conectify.Services.Automatization.Rules;

public class AndRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public int DefaultOutputs => 1;

    public IEnumerable<Tuple<InputTypeEnum, int>> DefaultInputs => new List<Tuple<InputTypeEnum, int>>() { new Tuple<InputTypeEnum, int>(InputTypeEnum.Value, 2), new Tuple<InputTypeEnum, int>(InputTypeEnum.Trigger, 1) };

    public Task Execute(RuleDTO masterRule, AutomatisationEvent trigger, CancellationToken ct)
    {
        var outputValue = new AutomatisationEvent()
        {
            Name = "AndRuleResult",
            NumericValue = !masterRule.Inputs.Any(x => x.AutomatisationValue is not null && x.AutomatisationValue.NumericValue == 0) ? 1 : 0,
            StringValue = !masterRule.Inputs.Any(x => x.AutomatisationValue is not null && x.AutomatisationValue.NumericValue == 0) ? "true" : "false",
            Unit = "",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = masterRule.Id
        };

        masterRule.SetAllOutputs(outputValue);

        return Task.CompletedTask;
    }

    public string DisplayName() => "AND Rule";

    public Guid GetId()
    {
        return Guid.Parse("28ff4530-887b-48d1-a4fa-38dc839257a4");
    }

    public Task InitializationValue(RuleDTO rule)
    {
        var outputValue = new AutomatisationEvent()
        {
            Name = "AndRuleResult",
            NumericValue = 0,
            StringValue = "false",
            Unit = "",
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SourceId = rule.Id
        };

        rule.SetAllOutputs(outputValue, false);
        return Task.CompletedTask;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    { 
    }
}
