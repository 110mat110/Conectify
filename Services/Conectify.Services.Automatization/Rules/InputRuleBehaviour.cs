using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class InputRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehavior
{
    public MinMaxDef Outputs => new(1, 1, 1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0,0,0)),
            new(InputTypeEnum.Trigger, new(0,0,0)),
            new(InputTypeEnum.Parameter, new(0,0,0))
        ];

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

    async Task IRuleBehavior.InitializationValue(RuleDTO rule, RuleDTO? ruleDTO)
    {
        if (await rule.SetAllOutputs(ruleDTO))
        {
            return;
        }

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

    public async Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var behaviour = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { SourceSensorId = Guid.Empty, Name = string.Empty, Event = string.Empty });
        if (behaviour is null || behaviour.SourceSensorId == Guid.Empty)
            return;

        var connectorService = serviceProvider.GetRequiredService<IConnectorService>();

        var sensor = await connectorService.LoadSensor(behaviour.SourceSensorId, cancellationToken);
        if (sensor is null)
        {
            return;
        }
        rule.ParametersJson = JsonConvert.SerializeObject(new { SourceSensorId = sensor.Id, sensor.Name, behaviour.Event });

        rule.Name = sensor.Name;
        rule.Description = "Source: " + sensor.Name;
    }

    internal InputRuleOptions? Options { get; set; }
}

internal class InputRuleOptions
{
    public string Event { get; set; } = string.Empty;

    public Guid SourceSensorId { get; set; }
}