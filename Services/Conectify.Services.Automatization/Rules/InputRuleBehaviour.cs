using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class InputRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehaviour
{
    public int DefaultOutputs => 1;

    public IEnumerable<Tuple<InputTypeEnum, int>> DefaultInputs => [new(InputTypeEnum.Value, 0), new(InputTypeEnum.Trigger, 0)];


    public string DisplayName() => "ON EVENT";

    public Guid GetId()
    {
        return Guid.Parse("24ff4530-887b-48d1-a4fa-38cc83925797");
    }

    public async Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        Options = JsonConvert.DeserializeObject<InputRuleOptions>(masterRule.ParametersJson);

        if(string.IsNullOrEmpty(Options?.Event) || Options.Event == Constants.Events.All || triggerValue.Type == Options.Event){
            await masterRule.SetAllOutputs(triggerValue);
        }
    }

    async Task IRuleBehaviour.InitializationValue(RuleDTO rule)
    {
        Options = JsonConvert.DeserializeObject<InputRuleOptions>(rule.ParametersJson);

        var connector = serviceProvider.GetRequiredService<IConnectorService>();
        var lastValue = await connector.LoadLastValue(Options!.SourceSensorId);

        if (lastValue is not null)
        {
            var evnt = new AutomatisationEvent()
            {
                Name = "Static value",
                NumericValue = lastValue.NumericValue,
                StringValue = lastValue.StringValue,
                Unit = lastValue.Unit,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SourceId = rule.Id,
            };

            await rule.SetAllOutputs(evnt, false);
        }
    }

    public void Clock(RuleDTO masterRule, TimeSpan interval, CancellationToken ct = default)
    {
    }

    internal InputRuleOptions? Options { get; set; }
}

internal class InputRuleOptions
{
    public string Event { get; set; } = string.Empty;

    public Guid SourceSensorId { get; set; }
}