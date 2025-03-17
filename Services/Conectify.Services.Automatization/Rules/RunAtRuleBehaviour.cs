using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class RunAtRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public MinMaxDef Outputs => new(1, 1, 1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0,0,0)),
            new(InputTypeEnum.Trigger, new(0,0,0)),
            new(InputTypeEnum.Parameter, new(0,0,0))
        ];

    public string DisplayName() => "RUN AT";
    public Guid GetId()
    {
        return Guid.Parse("3dff4530-887b-48d1-a4fa-38cc8392469a");
    }

    private class TimeRuleOptions
    {
        public DateTime TimeSet { get; set; }
    }

    public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
        var options = JsonConvert.DeserializeObject<TimeRuleOptions>(masterRule.ParametersJson);

        if (options is not null && DateTime.UtcNow > options.TimeSet && DateTime.UtcNow < options.TimeSet.Add(interval))
        {
            masterRule.SetAllOutputs(new AutomatisationEvent()
            {
                Name = "TimeRuleResult",
                NumericValue = 0,
                Unit = "",
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SourceId = masterRule.Id,
                Type = "Trigger",
            });
        }

    }

    Task IRuleBehaviour.InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        return Task.CompletedTask;
    }

    public Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
