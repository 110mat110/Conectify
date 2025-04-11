using Conectify.Database.Models;
using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

#pragma warning disable CS9113 // Parameter is unread. Need to be there due to BehaviourFactory
public class SetDelayBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
#pragma warning restore CS9113 // Parameter is unread.
{
    public MinMaxDef Outputs => new(1, 1, 1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0, 0, 0)),
        new(InputTypeEnum.Trigger, new(1, 1, 1)),
        new(InputTypeEnum.Parameter, new(0, 0, 0))
        ];

    public async void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
        var options = JsonConvert.DeserializeObject<SetDelayOptions>(masterRule.ParametersJson);

        if (options?.NextTrigger is not null && DateTime.UtcNow > options.NextTrigger.Value)
        {
            options.NextTrigger = null;
            masterRule.ParametersJson = JsonConvert.SerializeObject(options);

            await masterRule.SetAllOutputs(new AutomatisationEvent()
            {
                Name = "DelayRuleResult",
                NumericValue = 0,
                Unit = "",
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SourceId = masterRule.Id,
                Type = "Trigger",
            });
        }
    }

    public string DisplayName() => "SET DELAY";

    public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        var options = JsonConvert.DeserializeObject<SetDelayOptions>(masterRule.ParametersJson);

        if(options is null)
        {
            return Task.CompletedTask;
        }

        options.NextTrigger = DateTime.UtcNow.Add(options.Delay);
        masterRule.ParametersJson = JsonConvert.SerializeObject(options);

        return Task.CompletedTask;
    }

    public Guid GetId() => Guid.Parse("768fe726-caff-4120-a7f1-3d4c3c6817ac");

    public Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        if (oldDTO is not null)
        {
            rule.ParametersJson = oldDTO.ParametersJson;
        }

        return Task.CompletedTask;
    }

    public Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var options = JsonConvert.DeserializeObject<SetDelayOptions>(rule.ParametersJson);

        if(options?.Delay is not null)
        {
            rule.Description = $"Delay {options.Delay}";
        }

        return Task.CompletedTask;
    }

    private class SetDelayOptions
    {
        public DateTime? NextTrigger { get; set; }
        public TimeSpan Delay { get; set; }
    }
}
